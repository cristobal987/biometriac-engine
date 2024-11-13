using System;
using System.Collections.Generic;
using System.Text;
using DlibDotNet;

namespace BiometricEngine
{
    public class FaceModel
    {
        public string id_biometricos;
        public Matrix<float> modelo;
        public string nombre_archivo;
        public string direccion;
        public int longitud_modelo;

        public FaceModel() {
            id_biometricos = "";
            modelo = new Matrix<float>(128, 1);
            nombre_archivo = "";
            direccion = "";
            longitud_modelo = 0;
    }

        public FaceModel(string id_biometricos, Matrix<float> modelo)
        {
            this.id_biometricos = id_biometricos;
            this.modelo = modelo;
            nombre_archivo = "";
            direccion = "";
            longitud_modelo = 0;
        }

        public FaceModel(string id_biometricos, Matrix<float> modelo, string nombre_archivo, string direccion, int longitud_modelo)
        {
            this.id_biometricos = id_biometricos;
            this.modelo = modelo;
            this.nombre_archivo = nombre_archivo;
            this.direccion = direccion;
            this.longitud_modelo = longitud_modelo;
        }
    }
}
