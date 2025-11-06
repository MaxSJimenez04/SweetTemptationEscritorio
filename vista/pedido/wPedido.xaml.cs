using sweet_temptation_clienteEscritorio.dto;
using sweet_temptation_clienteEscritorio.model;
using sweet_temptation_clienteEscritorio.resources;
using sweet_temptation_clienteEscritorio.servicios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace sweet_temptation_clienteEscritorio.vista.pedido
{

    public partial class wPedido : Page
    {
        private PedidoService _servicioPedido;
        private Pedido _pedido;
        private int idUsuario = 3;

        public wPedido()
        {
            InitializeComponent();
            _servicioPedido = new PedidoService(new HttpClient());
            _pedido = new Pedido();
            Loaded += async (s, e) => await ObtenerPedidoActualAsync();
        }

        public void LlenarDatosPedido()
        {
            lbIdPedido.Content = _pedido.id;
            lbTotal.Content += $"{_pedido.total}";
            lbImpuesto.Content += $"{Constantes.IVA}%";
        }

        public void VaciarDatosPedido()
        {
            lbIdPedido.Content = "";
            lbTotal.Content = "Total: $";
            lbImpuesto.Content = "";
        }

        private async void btnClickCancelar(object sender, RoutedEventArgs e)
        {
            await CancelarPedidoAsync();
            await CrearPedidoClienteAsync();
        }

        private void btnClickRealizar(object sender, RoutedEventArgs e)
        {

        }

        public async Task ObtenerPedidoActualAsync()
        {
            var respuesta = await _servicioPedido.ObtenerPedidoActualAsync(idUsuario);
            if (respuesta.pedidoActual == null)
            {
                lbIdPedido.Content = "ERROR: No hay pedido actual";
            }
            else
            {
                _pedido.id = respuesta.pedidoActual.id;
                _pedido.actual = respuesta.pedidoActual.actual;
                _pedido.personalizado = respuesta.pedidoActual.personalizado;
                _pedido.estado = respuesta.pedidoActual.estado;
                _pedido.total = respuesta.pedidoActual.total;
                _pedido.fechaCompra = respuesta.pedidoActual.fechaCompra;
                _pedido.idCliente = respuesta.pedidoActual.idCliente;
                LlenarDatosPedido();
            }
        }

        public async Task CancelarPedidoAsync()
        {
            var respuesta = await _servicioPedido.CancelarPedidoAsync(_pedido.id, idUsuario);

            switch (respuesta.codigo)
            {
                case HttpStatusCode.OK:
                    VaciarDatosPedido();
                    MessageBox.Show("Pedido cancelado con éxito");
                    break;
                case HttpStatusCode.BadRequest:
                    MessageBox.Show("ERROR: ID no válido");
                    break;
                case HttpStatusCode.NotFound:
                    MessageBox.Show("ERROR: No se encontró el pedido actual");
                    break;
            }
        }

        public async Task CrearPedidoClienteAsync()
        {
            var respuesta = await _servicioPedido.CrearPedidoClienteAsync(idUsuario);
            if (respuesta)
            {
                await ObtenerPedidoActualAsync();
            }
            else
            {
                MessageBox.Show("ERROR: Ocurrió un error al cancelar el pedido");
            }
        }
    }
}
