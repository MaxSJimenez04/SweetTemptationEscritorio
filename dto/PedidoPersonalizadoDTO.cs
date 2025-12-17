namespace sweet_temptation_clienteEscritorio.dto {
    public class PedidoPersonalizadoDTO {
        public int id { get; set; }
        public string tamano { get; set; }
        public string saborBizcocho { get; set; }
        public string relleno { get; set; }
        public string cobertura { get; set; }
        public string especificaciones { get; set; }

        public string imagenUrl { get; set; }

        public DateTime fechaSolicitud { get; set; }
        public string telefonoContacto { get; set; }
        public int estado { get; set; }

        public string fechaFormateada {
            get {
                return fechaSolicitud.ToString("dd/MM/yyyy HH:mm");
            }
        }
        public string estadoTexto {
            get {
                switch(estado) {
                    case 0: return "Creado";
                    case 1: return "Enviado";
                    case 2: return "En Revisión";
                    case 3: return "Aceptado";
                    case 4: return "Rechazado";
                    default: return "Desconocido";
                }
            }
        }
    }
}