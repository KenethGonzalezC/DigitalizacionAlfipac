using BitacoraAlfipac.Models.Entidades;

namespace BitacoraAlfipac.Data.Seed
{
    public static class CatalogSeed
    {
        public static void SeedCatalogs(ApplicationDbContext context)
        {
            if (!context.PabMercanciasSusceptibles.Any())
            {
                context.PabMercanciasSusceptibles.AddRange(
                    new PabMercanciaSusceptible { Codigo = 1, Descripcion = "Rollos de bobinas de al menos 300 kg" },
                    new PabMercanciaSusceptible { Codigo = 2, Descripcion = "Estañones conteniendo aceites lubricantes" },
                    new PabMercanciaSusceptible { Codigo = 3, Descripcion = "Mármol en láminas de 20 o más metros" },
                    new PabMercanciaSusceptible { Codigo = 4, Descripcion = "Maquinaria que por sus dimensiones viaja en low boy" },
                    new PabMercanciaSusceptible { Codigo = 5, Descripcion = "Rollos de alambre con peso mayor a 200 kg" },
                    new PabMercanciaSusceptible { Codigo = 6, Descripcion = "Tubos de 15 metros de largo" },
                    new PabMercanciaSusceptible { Codigo = 7, Descripcion = "Gas refrigerante" },
                    new PabMercanciaSusceptible { Codigo = 8, Descripcion = "Mercancía a granel que corresponde a maíz" },
                    new PabMercanciaSusceptible { Codigo = 9, Descripcion = "Cajas de cartón presentadas a granel al piso" },
                    new PabMercanciaSusceptible { Codigo = 10, Descripcion = "Sacos de resina con un peso de 1 tonelada" },
                    new PabMercanciaSusceptible { Codigo = 11, Descripcion = "Mercancía peligrosa para la salud humana o medio ambiente (296 TER RLGA)" },
                    new PabMercanciaSusceptible { Codigo = 12, Descripcion = "Recomendación del MAG por falta de permisos de fumigación o etiquetado" },
                    new PabMercanciaSusceptible { Codigo = 13, Descripcion = "Razonamiento de la Gerencia de Aduana - susceptible de NO DESCARGA previo razonamiento" }
                );
            }

            if (!context.TransportistasAutorizados.Any())
            {
                context.TransportistasAutorizados.AddRange(
                    new TransportistaAutorizado { Nombre = "LOGISTICA DE TRANSPORTE INTERMODAL LTI SA", CedulaJuridica = "J-310125399931", Codigo = "GASH" },
                    new TransportistaAutorizado { Nombre = "MCV TRANSPORT OF COSTA RICA SOCIEDAD ANONIMA", CedulaJuridica = "J-310180827732", Codigo = "AMACAVI" },
                    new TransportistaAutorizado { Nombre = "COMPAÑÍA PEREZ ROJAS LIMITADA", CedulaJuridica = "J-310214408406", Codigo = "TRANS PR" },
                    new TransportistaAutorizado { Nombre = "KARPA FREIGHT FORWARDING SOCIEDAD ANONIMA", CedulaJuridica = "J-310116932224", Codigo = "H&H" },
                    new TransportistaAutorizado { Nombre = "RADA SOCIEDAD ANONIMA", CedulaJuridica = "J-310102421507", Codigo = "RADA / GUARUZA" },
                    new TransportistaAutorizado { Nombre = "ARAYA BRAVO & ASOCIADOS", CedulaJuridica = "J-310179208328", Codigo = "GUARUZA" },
                    new TransportistaAutorizado { Nombre = "TRANSPORTE BRAVO YUMBO SOCIEDAD ANONIMA", CedulaJuridica = "J-310172170922", Codigo = "YUMBO O ROMO" },
                    new TransportistaAutorizado { Nombre = "TRANSPORTES CENTROAMERICANOS DEL FUTURO INC S.A.", CedulaJuridica = "J-310168982819", Codigo = "TCF / TBM" },
                    new TransportistaAutorizado { Nombre = "TRANSPORTES HERMANOS BRAVO MAROTO S.A.", CedulaJuridica = "J-310138077408", Codigo = "TBM" },
                    new TransportistaAutorizado { Nombre = "ALAMO TERMINALES MARITIMOS SOCIEDAD ANONIMA", CedulaJuridica = "J-310124877436", Codigo = "ALAMO" },
                    new TransportistaAutorizado { Nombre = "ALVARADO Y GOMEZ SOCIEDAD ANONIMA", CedulaJuridica = "J-310102528812", Codigo = "TRAYGO" },
                    new TransportistaAutorizado { Nombre = "MEDTRUCKING LOGISTICS SERVICES SOCIEDAD ANONIMA", CedulaJuridica = "J-310182835722", Codigo = "MEDLOG" },
                    new TransportistaAutorizado { Nombre = "CORPORACION BUSTERS SOCIEDAD ANONIMA", CedulaJuridica = "J-310122981616", Codigo = "TERCONSA" },
                    new TransportistaAutorizado { Nombre = "SERVICIOS DE TRANSPORTE NAVIERO SETRANA", CedulaJuridica = "J-310163825207", Codigo = "SETRANA" },
                    new TransportistaAutorizado { Nombre = "TRANSPORTES SUPERIORES DEL ESTE ROLAN S.A.", CedulaJuridica = "J-310108304806", Codigo = "TRANSUP" },
                    new TransportistaAutorizado { Nombre = "EL TRIUNFO MERCANTIL", CedulaJuridica = "J-310260057631", Codigo = "TRANSECO" },
                    new TransportistaAutorizado { Nombre = "TRANSPORTES TRANSSOL S.R.L.", CedulaJuridica = "J-310276214603", Codigo = "TRASNSOL" },
                    new TransportistaAutorizado { Nombre = "TRANSPORTES Y SERVICIOS PORTUARIOS (TRANSPORT)", CedulaJuridica = "J-310170659535", Codigo = "TRASNPORT" },
                    new TransportistaAutorizado { Nombre = "TRANSPORTES TRANS COSTA RICA V E H S.A.", CedulaJuridica = "J-310108293619", Codigo = "TRANS C.R V E H" },
                    new TransportistaAutorizado { Nombre = "TRANSPORTE GUERRERO ANGULO SOCIEDAD ANONIMA", CedulaJuridica = "J-310169584202", Codigo = "GASA / AIMI-GASA" },
                    new TransportistaAutorizado { Nombre = "SOLUCIONES FAQUET INC SOCIEDAD ANONIMA", CedulaJuridica = "J-310168856922", Codigo = "FAQUET" },
                    new TransportistaAutorizado { Nombre = "RETICA SOCIEDAD ANONIMA", CedulaJuridica = "J-310168015613", Codigo = "RETICA" },
                    new TransportistaAutorizado { Nombre = "TRANSPOR VARGAS SOCIEDAD ANONIMA", CedulaJuridica = "J-310167985620", Codigo = "TRASNVARGAS" },
                    new TransportistaAutorizado { Nombre = "TRANSPORTES REFRIGERADOS H.L.", CedulaJuridica = "J-310245687512", Codigo = "T.H.L" },
                    new TransportistaAutorizado { Nombre = "ORTIZ ESPINOZA GUSTAVO", CedulaJuridica = "F-1118580826", Codigo = "PIRI-TRANSMATA-ROMO-SOLUTRANSA" },
                    new TransportistaAutorizado { Nombre = "SEAL TRADING GROUP", CedulaJuridica = "J-310166811107", Codigo = "SEAL TRADING" }
                );
            }

            context.SaveChanges();
        }
    }
}
