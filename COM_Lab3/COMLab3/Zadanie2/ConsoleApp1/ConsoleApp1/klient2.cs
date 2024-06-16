using System;


namespace klient2
{
    class klient2
    {
        public static void Main(string[] args)
        {
            Type type = Type.GetTypeFromProgID("KSR20.COM3Klasa.1");
            object act = Activator.CreateInstance(type);
            type.InvokeMember("Test", System.Reflection.BindingFlags.InvokeMethod, null, act, new object[] { "dziala klasa w c#" }); 
        }
    }
}
