using BiometricEngine;
using System.Drawing;

internal class Program
{
    static string root_path = "C:\\Temp\\recursos\\";
    static string foto_celebs_path = root_path + "celebs-foto2.jpg";
    static string foto_1_path = root_path + "Screenshot_1.png";
    static string foto_2_path = root_path + "Screenshot_2.png";
    static string foto_3_path = root_path + "Screenshot_3.png";
    static string pictures_bank = root_path + "lfw";
    static void Main(string[] args)
    {
        Console.WriteLine("Started");
        FaceEngine face_engine = new FaceEngine();
        try
        {
            //face_engine.RegenerateDataBase(pictures_bank);
            face_engine.LoadModelsFormDataBaseWithTimer();

            Bitmap bmp = new Bitmap(foto_3_path);
            ResultObject resultado = face_engine.Verify("20240107175245973", bmp);
            /*Bitmap bmp2 = new Bitmap(foto_3_path);

            ResultObject resultado = face_engine.Compare(bmp, bmp2);
*/
            Console.WriteLine("Resultado");

            Console.WriteLine(resultado.message);
            Console.WriteLine(resultado.state);
            Console.WriteLine(resultado.distance);
            Console.WriteLine(resultado.threshold);


        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }


        Console.ReadKey();
    }
}