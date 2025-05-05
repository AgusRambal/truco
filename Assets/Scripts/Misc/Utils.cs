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
        public static bool usarAprendizaje = false;
    }

    public static class Estadisticas
    {
        public static class Keys
        {
            public const string PartidasJugadas = "PartidasJugadas";
            public const string PartidasGanadas = "PartidasGanadas";
            public const string PartidasPerdidas = "PartidasPerdidas";
            public const string VecesQueTeFuiste = "VecesQueTeFuiste";
            public const string RachaVictoriasActual = "RachaVictoriasActual";
            public const string RachaVictoriasMaxima = "RachaVictoriasMaxima";

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
            var stats = SaveSystem.Datos.estadisticas;

            switch (key)
            {
                case Keys.PartidasJugadas: stats.partidasJugadas++; break;
                case Keys.VecesQueTeFuiste: stats.vecesQueTeFuiste++; break;

                case Keys.PartidasGanadas:
                    stats.partidasGanadas++;
                    stats.rachaVictoriasActual++;
                    if (stats.rachaVictoriasActual > stats.rachaVictoriasMaxima)
                        stats.rachaVictoriasMaxima = stats.rachaVictoriasActual;
                    break;

                case Keys.PartidasPerdidas:
                    stats.partidasPerdidas++;
                    stats.rachaVictoriasActual = 0;
                    break;

                case Keys.TrucosCantados: stats.trucosCantados++; break;
                case Keys.TrucosAceptados: stats.trucosAceptados++; break;
                case Keys.RetrucosCantados: stats.retrucosCantados++; break;
                case Keys.RetrucosAceptados: stats.retrucosAceptados++; break;
                case Keys.ValeCuatroCantados: stats.valeCuatroCantados++; break;
                case Keys.ValeCuatroAceptados: stats.valeCuatroAceptados++; break;

                case Keys.EnvidosCantados: stats.envidosCantados++; break;
                case Keys.EnvidosAceptados: stats.envidosAceptados++; break;
                case Keys.RealEnvidosCantados: stats.realEnvidosCantados++; break;
                case Keys.RealEnvidosAceptados: stats.realEnvidosAceptados++; break;
                case Keys.FaltaEnvidosCantados: stats.faltaEnvidosCantados++; break;
                case Keys.FaltaEnvidosAceptados: stats.faltaEnvidosAceptados++; break;

                case Keys.EnvidosGanados: stats.envidosGanados++; break;
                case Keys.RealEnvidosGanados: stats.realEnvidosGanados++; break;
                case Keys.FaltaEnvidosGanados: stats.faltaEnvidosGanados++; break;

                case Keys.EnvidosPerdidos: stats.envidosPerdidos++; break;
                case Keys.RealEnvidosPerdidos: stats.realEnvidosPerdidos++; break;
                case Keys.FaltaEnvidosPerdidos: stats.faltaEnvidosPerdidos++; break;

                case Keys.TrucosGanados: stats.trucosGanados++; break;
                case Keys.RetrucosGanados: stats.retrucosGanados++; break;
                case Keys.ValeCuatroGanados: stats.valeCuatroGanados++; break;

                case Keys.TrucosPerdidos: stats.trucosPerdidos++; break;
                case Keys.RetrucosPerdidos: stats.retrucosPerdidos++; break;
                case Keys.ValeCuatroPerdidos: stats.valeCuatroPerdidos++; break;

                default:
                    Debug.LogWarning($"Estadísticas: Key desconocida '{key}'");
                    return;
            }

            SaveSystem.GuardarDatos();
        }

        public static int Obtener(string key)
        {
            var stats = SaveSystem.Datos.estadisticas;

            return key switch
            {
                Keys.PartidasJugadas => stats.partidasJugadas,
                Keys.PartidasGanadas => stats.partidasGanadas,
                Keys.PartidasPerdidas => stats.partidasPerdidas,
                Keys.VecesQueTeFuiste => stats.vecesQueTeFuiste,
                Keys.RachaVictoriasActual => stats.rachaVictoriasActual,
                Keys.RachaVictoriasMaxima => stats.rachaVictoriasMaxima,

                Keys.TrucosCantados => stats.trucosCantados,
                Keys.TrucosAceptados => stats.trucosAceptados,
                Keys.RetrucosCantados => stats.retrucosCantados,
                Keys.RetrucosAceptados => stats.retrucosAceptados,
                Keys.ValeCuatroCantados => stats.valeCuatroCantados,
                Keys.ValeCuatroAceptados => stats.valeCuatroAceptados,

                Keys.EnvidosCantados => stats.envidosCantados,
                Keys.EnvidosAceptados => stats.envidosAceptados,
                Keys.RealEnvidosCantados => stats.realEnvidosCantados,
                Keys.RealEnvidosAceptados => stats.realEnvidosAceptados,
                Keys.FaltaEnvidosCantados => stats.faltaEnvidosCantados,
                Keys.FaltaEnvidosAceptados => stats.faltaEnvidosAceptados,

                Keys.EnvidosGanados => stats.envidosGanados,
                Keys.RealEnvidosGanados => stats.realEnvidosGanados,
                Keys.FaltaEnvidosGanados => stats.faltaEnvidosGanados,

                Keys.EnvidosPerdidos => stats.envidosPerdidos,
                Keys.RealEnvidosPerdidos => stats.realEnvidosPerdidos,
                Keys.FaltaEnvidosPerdidos => stats.faltaEnvidosPerdidos,

                Keys.TrucosGanados => stats.trucosGanados,
                Keys.RetrucosGanados => stats.retrucosGanados,
                Keys.ValeCuatroGanados => stats.valeCuatroGanados,

                Keys.TrucosPerdidos => stats.trucosPerdidos,
                Keys.RetrucosPerdidos => stats.retrucosPerdidos,
                Keys.ValeCuatroPerdidos => stats.valeCuatroPerdidos,

                _ => 0
            };
        }
    }
}
