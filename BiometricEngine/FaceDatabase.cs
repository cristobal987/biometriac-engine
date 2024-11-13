using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using DlibDotNet;
using MySql.Data.MySqlClient;

namespace BiometricEngine
{
    public class FaceDatabase:DataBase
    {
        public int Insert(string direccion, Matrix<float> modelo) {
            string idbiometricos = GenerateId();
            string nombrearchivo = getNombreArchivo(direccion);
            byte[] modelo_bytes = MatrixFloatToByteArray(modelo);
            MySqlCommand cmd = new MySqlCommand();
            cmd.CommandText = "INSERT INTO biometricos (idbiometricos, nombrearchivo, direccion, modelo, longitud_modelo ) VALUES ( @idbiometricos, @nombrearchivo, @direccion, @modelo, @longitud_modelo)";
            cmd.Parameters.AddWithValue("idbiometricos", idbiometricos);
            cmd.Parameters.AddWithValue("nombrearchivo", nombrearchivo);
            cmd.Parameters.AddWithValue("direccion", direccion);
            cmd.Parameters.AddWithValue("modelo", modelo_bytes);
            cmd.Parameters.AddWithValue("longitud_modelo", modelo.Size);
            int res = ExecuteNonQuery(cmd);

            cmd.Dispose();
            GC.Collect();
            return res;
        }

        public int Insert(string id,  string direccion, Matrix<float> modelo)
        {
            string idbiometricos = id;
            string nombrearchivo = getNombreArchivo(direccion);
            byte[] modelo_bytes = MatrixFloatToByteArray(modelo);
            MySqlCommand cmd = new MySqlCommand();
            cmd.CommandText = "INSERT INTO biometricos (idbiometricos, nombrearchivo, direccion, modelo, longitud_modelo ) VALUES ( @idbiometricos, @nombrearchivo, @direccion, @modelo, @longitud_modelo)";
            cmd.Parameters.AddWithValue("idbiometricos", idbiometricos);
            cmd.Parameters.AddWithValue("nombrearchivo", nombrearchivo);
            cmd.Parameters.AddWithValue("direccion", direccion);
            cmd.Parameters.AddWithValue("modelo", modelo_bytes);
            cmd.Parameters.AddWithValue("longitud_modelo", modelo.Size);
            int res = ExecuteNonQuery(cmd);

            cmd.Dispose();
            GC.Collect();
            return res;
        }

        public int Update(string sql) {
            return ExecuteNonQuery(sql);
        }

        public int Delete(string sql) {
            return ExecuteNonQuery(sql);
        }

        public DataTable FindAll() {
            string sql = "SELECT * FROM biometricos;";
            return ExecuteQuery(sql);
        }

        public byte[] MatrixFloatToByteArray(Matrix<float> matrix) {
            float[] array = matrix.ToArray();
            var byteArray = new byte[array.Length * 4];
            Buffer.BlockCopy(array, 0, byteArray, 0, byteArray.Length);
            return byteArray;
        }

        public Matrix<float> ByteArrayToMatrixFloat(byte[] byteArray) {
            var floatArray = new float[byteArray.Length / 4];
            Buffer.BlockCopy(byteArray, 0, floatArray, 0, byteArray.Length);

            return new Matrix<float>(floatArray, 128, 1); ;
        }

        public string getNombreArchivo(string path) {
            string[] array = path.Split("\\");
            int last = array.Length - 1;
            return array[last];
        }
    }
}
