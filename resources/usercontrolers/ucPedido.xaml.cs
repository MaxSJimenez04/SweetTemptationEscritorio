using sweet_temptation_clienteEscritorio.model;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace sweet_temptation_clienteEscritorio.resources.usercontrolers
{
    /// <summary>
    /// Lógica de interacción para ucPedido.xaml
    /// </summary>
    public partial class ucPedido : UserControl
    {
        private Pedido _pedido;
        public event EventHandler<Pedido> Eliminar;
        public event Action<Pedido> Modificar;
        public int id => _pedido.id;
        public ucPedido(Pedido pedido)
        {
            InitializeComponent();
            _pedido = pedido;
            LlenarDatos();
        }

        public void LlenarDatos()
        {
            lbId.Content =$"Orden: {_pedido.id}";
            lbTotal.Content = $"Total: ${_pedido.total}";
        }

        private void BtnClickEditar(object sender, RoutedEventArgs e)
        {
            Modificar?.Invoke(_pedido);
        }

        private async void BtnClickEliminar(object sender, RoutedEventArgs e)
        {
            Eliminar?.Invoke(this, _pedido);
        }
    }
}
