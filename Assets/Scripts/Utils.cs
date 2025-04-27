using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static class ParametrosDePartida
    {
        public static int puntosParaGanar = 30;
        public static int gananciaCalculada = 10;
        public static EstiloIA estiloSeleccionado = EstiloIA.Canchero;
        public static List<CartaSO> cartasSeleccionadas = new();
    }

    public static class Estadisticas
    {
        public static class Keys
        {
            public const string PartidasJugadas = "PartidasJugadas";
            public const string PartidasGanadas = "PartidasGanadas";
            public const string PartidasPerdidas = "PartidasPerdidas";
            public const string VecesQueTeFuiste = "VecesQueTeFuiste";

            public const string TrucosCantados = "TrucosCantados";
            public const string TrucosAceptados = "TrucosAceptados";
            public const string RetrucosCantados = "RetrucosCantados";
            public const string RetrucosAceptados = "RetrucosAceptados";
            public const string ValeCuatroCantados = "ValeCuatroCantados";
            public const string ValeCuatroAceptados = "ValeCuatroAceptados";

            public const string EnvidosCantados = "EnvidosCantados";
            public const string EnvidosAceptados = "EnvidosAceptados";
            public const string RealEnvidosCantados = "RealEnvidosCantados";
            public const string RealEnvidosAceptados = "RealEnvidosAceptados";
            public const string FaltaEnvidosCantados = "FaltaEnvidosCantados";
            public const string FaltaEnvidosAceptados = "FaltaEnvidosAceptados";

            public const string EnvidosGanados = "EnvidosGanados";
            public const string RealEnvidosGanados = "RealEnvidosGanados";
            public const string FaltaEnvidosGanados = "FaltaEnvidosGanados";

            public const string EnvidosPerdidos = "EnvidosPerdidos";
            public const string RealEnvidosPerdidos = "RealEnvidosPerdidos";
            public const string FaltaEnvidosPerdidos = "FaltaEnvidosPerdidos";

            public const string TrucosGanados = "TrucosGanados";
            public const string RetrucosGanados = "RetrucosGanados";
            public const string ValeCuatroGanados = "ValeCuatroGanados";

            public const string TrucosPerdidos = "TrucosPerdidos";
            public const string RetrucosPerdidos = "RetrucosPerdidos";
            public const string ValeCuatroPerdidos = "ValeCuatroPerdidos";
        }

        public static void Sumar(string key)
        {
            int valor = PlayerPrefs.GetInt(key, 0);
            PlayerPrefs.SetInt(key, valor + 1);
            PlayerPrefs.Save();
        }

        public static int Obtener(string key)
        {
            return PlayerPrefs.GetInt(key, 0);
        }

        /*public static void Resetear()
        {
            PlayerPrefs.DeleteKey(Keys.PartidasJugadas);
            PlayerPrefs.DeleteKey(Keys.PartidasGanadas);
            PlayerPrefs.DeleteKey(Keys.PartidasPerdidas);
            PlayerPrefs.DeleteKey(Keys.VecesQueTeFuiste);

            PlayerPrefs.DeleteKey(Keys.TrucosCantados);
            PlayerPrefs.DeleteKey(Keys.TrucosAceptados);
            PlayerPrefs.DeleteKey(Keys.RetrucosCantados);
            PlayerPrefs.DeleteKey(Keys.RetrucosAceptados);
            PlayerPrefs.DeleteKey(Keys.ValeCuatroCantados);
            PlayerPrefs.DeleteKey(Keys.ValeCuatroAceptados);

            PlayerPrefs.DeleteKey(Keys.EnvidosCantados);
            PlayerPrefs.DeleteKey(Keys.EnvidosAceptados);
            PlayerPrefs.DeleteKey(Keys.RealEnvidosCantados);
            PlayerPrefs.DeleteKey(Keys.RealEnvidosAceptados);
            PlayerPrefs.DeleteKey(Keys.FaltaEnvidosCantados);
            PlayerPrefs.DeleteKey(Keys.FaltaEnvidosAceptados);

            PlayerPrefs.DeleteKey(Keys.EnvidosGanados);
            PlayerPrefs.DeleteKey(Keys.RealEnvidosGanados);
            PlayerPrefs.DeleteKey(Keys.FaltaEnvidosGanados);

            PlayerPrefs.DeleteKey(Keys.EnvidosPerdidos);
            PlayerPrefs.DeleteKey(Keys.RealEnvidosPerdidos);
            PlayerPrefs.DeleteKey(Keys.FaltaEnvidosPerdidos);

            PlayerPrefs.DeleteKey(Keys.TrucosGanados);
            PlayerPrefs.DeleteKey(Keys.RetrucosGanados);
            PlayerPrefs.DeleteKey(Keys.ValeCuatroGanados);

            PlayerPrefs.DeleteKey(Keys.TrucosPerdidos);
            PlayerPrefs.DeleteKey(Keys.RetrucosPerdidos);
            PlayerPrefs.DeleteKey(Keys.ValeCuatroPerdidos);


            PlayerPrefs.Save();
        }*/
    }
}
