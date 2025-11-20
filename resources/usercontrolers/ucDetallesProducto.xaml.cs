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

    public partial class ucDetallesProducto : UserControl
    {
        private DetallesProducto _detallesProducto;
        public event Action<DetallesProducto> OnEliminar;
        public DetallesProducto detalles;

        public ucDetallesProducto(DetallesProducto detallesProducto)
        {
            _detallesProducto = detallesProducto;
            InitializeComponent();
            LlenarDatos();
            detalles = _detallesProducto;
        }

        public void ColocarImagen(BitmapImage imagen)
        {
            imgProducto.Source = imagen;
        }

        private void LlenarDatos()
        {
            lbNombre.Content = _detallesProducto.nombre;
            lbPrecio.Content = _detallesProducto.precio;
            lbCantidad.Content = $"Cantidad: {_detallesProducto.cantidad}";
            lbNumero.Content = _detallesProducto.cantidad;
        }

        public void HabilitarEdicion()
        {
            spSpinner.Visibility = Visibility.Visible;
            lbCantidad.Visibility = Visibility.Collapsed;
            btnEliminar.Visibility = Visibility.Visible;
        }

        public void DeshabilitarEdicion()
        {
            spSpinner.Visibility = Visibility.Collapsed;
            lbCantidad.Visibility = Visibility.Visible;
            btnEliminar.Visibility = Visibility.Collapsed;
        }

        private void btnClickAgregar(object sender, RoutedEventArgs e)
        {
            _detallesProducto.cantidad = _detallesProducto.cantidad + 1;
            lbNumero.Content = _detallesProducto.cantidad;
            lbCantidad.Content = $"Cantidad: {_detallesProducto.cantidad}";
            _detallesProducto.subtotal = _detallesProducto.precio * _detallesProducto.cantidad;
        }

        private void btnClickRestar(object sender, RoutedEventArgs e)
        {
            _detallesProducto.cantidad = _detallesProducto.cantidad - 1;
            lbNumero.Content = _detallesProducto.cantidad;
            lbCantidad.Content = $"Cantidad: {_detallesProducto.cantidad}";
            _detallesProducto.subtotal = _detallesProducto.precio * _detallesProducto.cantidad;
        }

        private void btnClickEliminar(object sender, RoutedEventArgs e)
        {
            OnEliminar?.Invoke(_detallesProducto);
        }
    }
}
