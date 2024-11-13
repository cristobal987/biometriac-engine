using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using DlibDotNet;

namespace BiometricEngine
{
    public class FaceEngine:Face
    {
        private List<FaceModel> faces_models;
        private FaceDatabase face_database;

        public FaceEngine() {
            faces_models = new List<FaceModel>();
            face_database = new FaceDatabase();
        }

        /// <summary>
        /// Agrega un FaceModel a la lista
        /// </summary>
        /// <param name="face_model"></param>
        public void AddFaceModel(FaceModel face_model)
        {
            faces_models.Add(face_model);
        }
        /// <summary>
        /// Agrega un FaceModel a la lista
        /// </summary>
        /// <param name="id"></param>
        /// <param name="distances"></param>
        public void AddFaceModel(string id, Matrix<float> distances)
        {
            faces_models.Add(new FaceModel(id, distances));
        }

        /// <summary>
        /// Lee una imagen del directorio, extrae la pantilla del rostro encontrado y la 
        /// inserta en la base de datos
        /// </summary>
        /// <param name="image_path"></param>
        public void AddFaceModelToDataBAseFromFile(string image_path) {
            var distances = ExtractTemplate(image_path);
            if (distances == null) return;

            string id = face_database.GenerateId();

            face_database.Insert(id, image_path, distances);
            distances.Dispose();
            GC.Collect();
        }

        /// <summary>
        /// Devuelve la cantidad de plantillas cargadas en memoria
        /// </summary>
        /// <returns></returns>
        public int GetFaceModelsCount() {
            return (faces_models == null)? 0 : faces_models.Count;
        }

        /// <summary>
        /// Lee la lista de platillas en la base de datos y las carga a memoria
        /// </summary>
        public void LoadModelsFormDataBase()
        {
            faces_models.Clear();
            DataTable datatable = face_database.FindAll();
            foreach (DataRow row in datatable.Rows)
            {
                AddFaceModel(ParseRowToFaceModel(row));
            }
            Console.WriteLine(GetFaceModelsCount() + " models loaded");
        }

        public void LoadModelsFormDataBaseWithTimer() {
            DateTime init = DateTime.Now;
            LoadModelsFormDataBase();
            DateTime end = DateTime.Now;
            TimeSpan timer = end - init;
            Console.WriteLine("Duration " + timer);
        }

        /// <summary>
        /// Lee las imagenes en el directorio de imagenes y las inserta en la base de datos.
        /// como parametros se debe indicar la direcion del banco de imagenes, esta carpeta
        /// debe tener la siguiente estructura "banco_imagenes\identificador\nombre_archivo.jpg" 
        /// </summary>
        /// <param name="path">direccion del banco de imagenes</param>
        public void RegenerateDataBase(string path) {
            var directories = Directory.EnumerateDirectories(path);
            int directoriescounter = 0;
            int filescounter = 0;
            IEnumerable<string> files;
            foreach (var directory in directories)
            {
                files = Directory.EnumerateFiles(directory);
                foreach (var file in files)
                {
                    AddFaceModelToDataBAseFromFile(file);
                    filescounter++;
                    break;
                }
                directoriescounter++;
            }
            Console.WriteLine(directoriescounter + " directories and " + filescounter + " images");
        }

        /// <summary>
        /// Funcion para regenerar una base de datos de plantillas biometricas con un temporizador chafa
        /// </summary>
        /// <param name="path"></param>
        public void RegenerateDataBaseWithTimer(string path) {
            DateTime init = DateTime.Now;
            RegenerateDataBase(path);
            DateTime end = DateTime.Now;
            TimeSpan timer = end - init;
            Console.WriteLine("Duration " + timer);
        }

        /// <summary>
        /// Convierte una fila de la tabla Biometricos en un objeto FaceModel
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        private FaceModel ParseRowToFaceModel(DataRow row)
        {
            var array = row.ItemArray;
            string id = (string)array.GetValue(0);
            string nombrearchivo = (string)array.GetValue(1);
            string direccion = (string)array.GetValue(2);
            int longitud = (int)array.GetValue(4);

            byte[] byteArray = (byte[])array.GetValue(3);
            Matrix<float> distances = face_database.ByteArrayToMatrixFloat(byteArray);

            return new FaceModel(id, distances, nombrearchivo, direccion, longitud);
        }

        /// <summary>
        /// Funcion que implementa EucledianDistance para evaluar la similitud de 
        /// dos plantillas
        /// </summary>
        /// <param name="template1"></param>
        /// <param name="template2"></param>
        /// <returns></returns>
        public ResultObject Compare(Matrix<float> template1, Matrix<float> template2)
        {
            float distance = EucledianDistance(template1, template2);
            float threshold = getThreshold();
            bool status = distance < threshold;

            string message = "Rostros diferentes";
            if (status == true)
            {
                message = "Rostros iguales";
            }
            return new ResultObject(status, message, threshold, distance);
        }

        /// <summary>
        /// Funcion que implementa EucledianDistance para evaluar la similitud de 
        /// dos imagenes
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="bmp2"></param>
        /// <returns></returns>
        public ResultObject Compare(Bitmap bmp, Bitmap bmp2)
        {
            var template1 = ExtractTemplate(bmp);
            var template2 = ExtractTemplate(bmp2);
            float distance = EucledianDistance(template1, template2);
            float threshold = getThreshold();
            bool status = distance < threshold;

            string message = "Rostros diferentes";
            if (status == true) {
                message = "Rostros iguales";
            }
            return new ResultObject(status, message, threshold, distance);
        }

        /// <summary>
        /// Funcion que busca el rostro en toda la base de datos de biometricos y devuelve el rostro mas parecido (1 - n)
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public ResultObject Validate(Bitmap image)
        {
            if (GetFaceModelsCount() == 0) {
                return new ResultObject(false, "no hay plantillas cargadas en el motor", 0f, 0f);
            }
            var template1 = ExtractTemplate(image);
            List<ResultObject> array = new List<ResultObject>();
            faces_models.ForEach(model => {
                float distancia = EucledianDistance(model.modelo, template1);
                if(distancia < getThreshold())
                {
                    array.Add(new ResultObject(true, "rostro encontrado", getThreshold(), distancia));
                }
            });

            int modelsCount = (array == null) ? 0 : array.Count;
            if(modelsCount == 0)
            {
                return new ResultObject(false, "no se encontro el rostro en la base de datos", 0f, 0f);
            }

            if(modelsCount == 1)
            {
                return array.First();
            }
            else
            {
                Console.WriteLine("mas de uno parecido : " + modelsCount);
                ResultObject res = new ResultObject(false, "", 0f, 0f);
                float menor = 100f;
                array.ForEach(item =>
                {
                    if (item.distance < menor)
                    {
                        menor = item.distance;
                        res = item;
                    }
                });
                return res;
            }

        }

        /// <summary>
        /// funcion que busca en la base de datos primero con el identificador y luego con la biometria asignada (1 - 1)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="image"></param>
        /// <returns></returns>
        public ResultObject Verify(string id, Bitmap image)
        {
            if (GetFaceModelsCount() == 0)
            {
                return new ResultObject(false, "no hay plantillas cargadas en el motor", 0f, 0f);
            }

            var template1 = ExtractTemplate(image);
            List<ResultObject> array = new List<ResultObject>();

            faces_models.ForEach(model => {
                if (model.id_biometricos == id)
                {
                    float distancia = EucledianDistance(model.modelo, template1);
                    if (distancia < getThreshold())
                    {
                        array.Add(new ResultObject(true, "rostro encontrado", getThreshold(), distancia));
                    }
                }
                
            });

            int modelsCount = (array == null) ? 0 : array.Count;
            if (modelsCount == 0)
            {
                return new ResultObject(false, "El rostro no corresponde con el id proporcionado", 0f, 0f);
            }

            return array.First();
            
        }
    }
}
