using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DlibDotNet;
using DlibDotNet.Dnn;


namespace BiometricEngine
{
    public class Face
    {
        private static string root_path = "C:\\Temp\\recursos\\";
        private static string pose_predictor_path = root_path + "shape_predictor_5_face_landmarks.dat";
        private static string face_encoder_path = root_path + "dlib_face_recognition_resnet_model_v1.dat";
        private static float threshold = 0.5f;

        /// <summary>
        /// Funcion que convierte una imagen bmp a una matriz RgbPixel,
        /// este formato es con el cual trabaja la liberira dlib para
        /// la deteccion y reconocimiento de rostros
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public Matrix<RgbPixel> ToMatrixRgbPixel(Bitmap bitmap)
        {
            int columns = bitmap.Width;
            int rows = bitmap.Height;
            var matrix = new Matrix<RgbPixel>(rows, columns);
            RgbPixel[] pixel_array = new RgbPixel[rows * columns];
            RgbPixel temp_pixel = new RgbPixel();
            Color color;
            int position = 0;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    color = bitmap.GetPixel(j, i);
                    temp_pixel.Blue = color.B;
                    temp_pixel.Red = color.R;
                    temp_pixel.Green = color.G;
                    pixel_array[position] = temp_pixel;
                    position++;
                }
            }
            matrix.Assign(pixel_array);
            return matrix;
        }

        /// <summary>
        /// Funcion que detecta los rostros en la imagen, la funcion GetFrontalFaceDetector
        /// por defecto usa el algoritmo HOG (Histogram of Oriented Gradients) para detectar
        /// rostros en la imagen, el resultado de la funcion es un arreglo de rectangulos de
        /// los rostros encontrados
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        private DlibDotNet.Rectangle[] DetectFacesInImage(Matrix<RgbPixel> img) {
            DlibDotNet.Rectangle[] rectangles = null;
            using (FrontalFaceDetector detector2 = Dlib.GetFrontalFaceDetector()) {
                rectangles = detector2.Operator(img);
            }
            return rectangles;
        }

        /// <summary>
        /// Funcion que dada una imagen y un rectangulo extrae los landmarks
        /// del rostro
        /// </summary>
        /// <param name="img"></param>
        /// <param name="face"></param>
        /// <returns></returns>
        private Matrix<RgbPixel> GetFaceChip(Matrix<RgbPixel> img, DlibDotNet.Rectangle face) {
            Matrix<RgbPixel> faceChip;
            using (ShapePredictor shapePredictor2 = ShapePredictor.Deserialize(pose_predictor_path)) {
                var shape = shapePredictor2.Detect(img, face);
                var faceChipDetail = Dlib.GetFaceChipDetails(shape, 150, 0.25);
                faceChip = Dlib.ExtractImageChip<RgbPixel>(img, faceChipDetail);

                shape.Dispose();
                faceChipDetail.Dispose();
            }
            return faceChip;
        }

        /// <summary>
        /// Funcion que implementa las funciones DetectFacesInImage y GetFaceChip 
        /// para detectar los rostros en una imagen, y devuelve una lista con los 
        /// rostros en formato Matrix<RgbPixel>
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public List<Matrix<RgbPixel>> FindFaces(Bitmap bmp) {
            var faces = new List<Matrix<RgbPixel>>();
            var img = ToMatrixRgbPixel(bmp);
            foreach (var face in DetectFacesInImage(img))
            {
                faces.Add(GetFaceChip(img, face));
            }
            return faces;
        }

        /// <summary>
        /// Funcion que implementa las funciones DetectFacesInImage y GetFaceChip 
        /// para detectar los rostros en una imagen, y devuelve una lista con los 
        /// rostros en formato Matrix<RgbPixel>
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public List<Matrix<RgbPixel>> FindFaces(Matrix<RgbPixel> img)
        {
            var faces = new List<Matrix<RgbPixel>>();
            foreach (var face in DetectFacesInImage(img))
            {
                faces.Add(GetFaceChip(img, face));
            }
            return faces;
        }

        /// <summary>
        /// Funcion que implementa las funciones DetectFacesInImage y GetFaceChip 
        /// para detectar los rostros en una imagen, y devuelve el primer rostro
        /// encontrado en la imagen en formato Matrix<RgbPixel>
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public Matrix<RgbPixel> FindFace(Matrix<RgbPixel> img)
        {
            var faces = new List<Matrix<RgbPixel>>();
            foreach (var face in DetectFacesInImage(img))
            {
                faces.Add(GetFaceChip(img, face));
            }
            return faces.FirstOrDefault();
        }

        /// <summary>
        /// Funcion que implementa las funciones DetectFacesInImage y GetFaceChip 
        /// para detectar los rostros en una imagen, y devuelve el primer rostro
        /// encontrado en la imagen en formato Matrix<RgbPixel>
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public Matrix<RgbPixel> FindFace(Bitmap bmp)
        {
            var img = ToMatrixRgbPixel(bmp);
            var faces = new List<Matrix<RgbPixel>>();
            foreach (var face in DetectFacesInImage(img))
            {
                faces.Add(GetFaceChip(img, face));
            }
            img.Dispose();
            bmp.Dispose();
            return faces.FirstOrDefault();
        }

        /// <summary>
        /// Esta funcion dado un rostro encontrado extraer 128 puntos de referencia del rostro
        /// que funcionan como plantilla para hacer el reconocimiento facial, 128 puntos estan 
        /// representado como un arreglo de numeros flotantes (float)
        /// </summary>
        /// <param name="face"></param>
        /// <returns></returns>
        private OutputLabels<Matrix<float>> GetFaceLabels(Matrix<RgbPixel> face) {
            OutputLabels<Matrix<float>> output;
            using (LossMetric net2 = LossMetric.Deserialize(face_encoder_path)) {
                output = net2.Operator(face);
            }
            return output;
        }

        /// <summary>
        /// Funcion que implementa FindFace y GetFaceLabels para extraer las plantillas (128 puntos)
        /// de todos los rostros encontrados en una imagen
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public OutputLabels<Matrix<float>> ExtractTemplates(Bitmap bmp) {
            OutputLabels<Matrix<float>> output;
            try
            {
                var faces = FindFace(bmp);
                if (faces == null || !faces.Any())
                {
                    throw new Exception();
                }
                output = GetFaceLabels(faces);
            }
            catch (Exception e)
            {
                Console.WriteLine("No face found in image! ");
                Console.WriteLine(e.Message);
                output = null;
            }
            return output;
        }

        /// <summary>
        /// Funcion que implementa FindFace y GetFaceLabels para extraer la plantilla (128 puntos)
        /// del primer rostro encontrado en una imagen
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public Matrix<float> ExtractTemplate(Matrix<RgbPixel> bmp)
        {
            Matrix<float> output;
            try
            {
                var faces = FindFace(bmp);
                if (faces == null || !faces.Any())
                {
                    throw new Exception();
                }
                output = GetFaceLabels(faces)?.FirstOrDefault();
            }
            catch (Exception e)
            {
                Console.WriteLine("No face found in image! ");
                Console.WriteLine(e.Message);
                output = null;
            }
            return output;
        }

        /// <summary>
        /// Funcion que implementa FindFace y GetFaceLabels para extraer la plantilla (128 puntos)
        /// del primer rostro encontrado en una imagen
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public Matrix<float> ExtractTemplate(Bitmap bmp)
        {
            Matrix<float> output;
            try
            {
                var faces = FindFace(bmp);
                if (faces == null || !faces.Any())
                {
                    throw new Exception();
                }
                output = GetFaceLabels(faces)?.FirstOrDefault();
            }
            catch (Exception e)
            {
                Console.WriteLine("No face found in image! ");
                Console.WriteLine(e.Message);
                output = null;
            }
            return output;
        }

        /// <summary>
        /// Funcion que implementa FindFace y GetFaceLabels para extraer la plantilla (128 puntos)
        /// del primer rostro encontrado en una imagen
        /// </summary>
        /// <param name="image_path"></param>
        /// <returns></returns>
        public Matrix<float> ExtractTemplate(string image_path)
        {
            Matrix<float> output;
            Bitmap bmp = new Bitmap(image_path);
            try
            {
                var faces = FindFace(bmp);
                if (faces == null || !faces.Any())
                {
                    throw new Exception();
                }
                output = GetFaceLabels(faces)?.FirstOrDefault();
            }
            catch (Exception e) {
                Console.WriteLine("No face found in image! " + image_path);
                Console.WriteLine(e.Message);
                output = null;
            }
            return output;
        }

        /// <summary>
        /// Esta funcion calcula la distancia euclidiana entre dos arreglos unidimensionales 
        /// (plantillas) y da como resultado un numero flotante, que mientras mas cercano a 
        /// 0 sea el resultado mas parecidos son los rostros a los que corresponden dichas 
        /// plantillas
        /// </summary>
        /// <param name="template1"></param>
        /// <param name="template2"></param>
        /// <returns></returns>
        public float EucledianDistance(Matrix<float> template1, Matrix<float> template2) {
            var diff = template1 - template2;
            return Dlib.Length(diff);
        }

        /// <summary>
        /// Lee una imagen en el directorio y la carga memoria en formato Matrix<RgbPixel>
        /// </summary>
        /// <param name="imagePath"></param>
        /// <returns></returns>
        public Matrix<RgbPixel> LoadImagesAsMatrix(string imagePath) {
            return Dlib.LoadImageAsMatrix<RgbPixel>(imagePath);
        }

        /// <summary>
        /// Lee una imagen en el directorio y la carga memoria como Bitmap y la convierte
        /// a Matrix<RgbPixel>. Este metodo es mas lento pero es el que mejor resultados da
        /// a la hora de detectar rostros
        /// </summary>
        /// <param name="imagePath"></param>
        /// <returns></returns>
        public Matrix<RgbPixel> LoadBitmapAsMatrix(string imagePath)
        {
            Bitmap bitmap = new Bitmap(imagePath);
            return ToMatrixRgbPixel(bitmap);
        }

        public float getThreshold() {
            return threshold;
        }
    }
}
