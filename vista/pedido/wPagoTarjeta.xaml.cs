using sweet_temptation_clienteEscritorio.dto;
using sweet_temptation_clienteEscritorio.servicios;
using System;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using sweet_temptation_clienteEscritorio.model;

namespace sweet_temptation_clienteEscritorio.vista.pedido {
    public partial class wPagoTarjeta : Page, INotifyPropertyChanged {
        private string _numeroTarjeta;
        private string _titular;
        private string _vencimiento;
        private string _cvc;
        private bool _isFormatting = false;
        private Pedido _pedido;
        private readonly PagoService pagoService;
        private readonly ProductoPedidoService productoPedidoService;
        private TicketGrpcService ticketService;

        public string NumeroTarjeta {
            get => _numeroTarjeta;
            set {
                _numeroTarjeta = value;
                OnPropertyChanged(nameof(NumeroTarjeta));
            }
        }
        public string Titular {
            get => _titular;
            set {
                _titular = value == null ? null : value.TrimStart();
                OnPropertyChanged(nameof(Titular));
            }
        }

        public string Vencimiento {
            get => _vencimiento;
            set {
                _vencimiento = value;
                OnPropertyChanged(nameof(Vencimiento));
            }
        }

        public string CVC {
            get => _cvc;
            set {
                _cvc = value;
                OnPropertyChanged(nameof(CVC));
            }
        }

        public wPagoTarjeta(Pedido pedido) {
            InitializeComponent();
            DataContext = this;
            _pedido = pedido;
            pagoService = new PagoService(new HttpClient());
            productoPedidoService = new ProductoPedidoService(new HttpClient());
            ticketService = new TicketGrpcService();
            LlenarDatosPedido();
        }

        private void TxtNumero_PreviewTextInput(object sender, TextCompositionEventArgs e) {
            e.Handled = !Regex.IsMatch(e.Text, "^[0-9]+$");
        }

        private void TxtNumero_TextChanged(object sender, TextChangedEventArgs e) {
            if(_isFormatting) return;
            var txt = sender as TextBox;
            if(txt == null) return;

            _isFormatting = true;

            int originalSelectionStart = txt.SelectionStart;
            string oldText = txt.Text ?? string.Empty;
            int spacesBeforeCaret = oldText.Substring(0, Math.Min(originalSelectionStart, oldText.Length))
                                         .Count(ch => ch == ' ');
            int rawCaretIndex = Math.Max(0, originalSelectionStart - spacesBeforeCaret);

            string raw = Regex.Replace(oldText, @"\s+", "");
            if(raw.Length > 16) raw = raw.Substring(0, 16);

            NumeroTarjeta = raw; 

            string formatted = "";
            for(int i = 0; i < raw.Length; i++) {
                if(i > 0 && i % 4 == 0) formatted += " ";
                formatted += raw[i];
            }

            int spacesBeforeNewCaret = rawCaretIndex / 4;
            int newCaret = rawCaretIndex + spacesBeforeNewCaret;
            if(newCaret > formatted.Length) newCaret = formatted.Length;

            if(txt.Text != formatted) {
                txt.Text = formatted;
                txt.SelectionStart = newCaret;
            }

            _isFormatting = false;
        }

        private void TxtTitular_PreviewTextInput(object sender, TextCompositionEventArgs e) {
            var txt = sender as TextBox;
            if(txt == null) return;

            int selStart = txt.SelectionStart;

            if(selStart == 0) {
                e.Handled = !Regex.IsMatch(e.Text, "^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ]$");
            } else {
                e.Handled = !Regex.IsMatch(e.Text, "^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ\\s]$");
            }
        }

        private void TxtTitular_PreviewKeyDown(object sender, KeyEventArgs e) {
            if(e.Key == Key.Space) {
                var txt = sender as TextBox;
                if(txt != null && txt.SelectionStart == 0) {
                    e.Handled = true;
                }
            }
        }

        private void TxtTitular_Pasting(object sender, DataObjectPastingEventArgs e) {
            if(!e.DataObject.GetDataPresent(DataFormats.Text)) {
                e.CancelCommand();
                return;
            }

            string pasted = e.DataObject.GetData(DataFormats.Text) as string ?? string.Empty;

            string sanitized = Regex.Replace(pasted, @"^\s+", ""); 
            sanitized = Regex.Replace(sanitized, @"[^a-zA-ZáéíóúÁÉÍÓÚüÜñÑ\s]", ""); 

            if(string.IsNullOrEmpty(sanitized) || !Regex.IsMatch(sanitized.Substring(0, 1), "^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ]$")) {
                e.CancelCommand();
                return;
            }

            var txt = sender as TextBox;
            if(txt == null) {
                e.CancelCommand();
                return;
            }

            int selStart = txt.SelectionStart;
            int selLen = txt.SelectionLength;
            string newText = txt.Text.Remove(selStart, selLen).Insert(selStart, sanitized);
            txt.Text = newText;
            txt.SelectionStart = selStart + sanitized.Length;

            e.CancelCommand(); 
        }

        private void TxtCVC_PreviewTextInput(object sender, TextCompositionEventArgs e) {
            e.Handled = !Regex.IsMatch(e.Text, "^[0-9]+$");
        }

        private void TxtVencimiento_PreviewTextInput(object sender, TextCompositionEventArgs e) {
            e.Handled = !Regex.IsMatch(e.Text, "^[0-9]+$");
        }

        private void TxtVencimiento_PreviewKeyDown(object sender, KeyEventArgs e) {
            var txt = sender as TextBox;
            if(txt == null) return;

            if(e.Key != Key.Back) return;

            int caret = txt.CaretIndex;

            if(caret == 3 && txt.Text.Length >= 3 && txt.Text[2] == '/') {
                txt.Text = txt.Text.Substring(0, 1); 
                txt.CaretIndex = 1;

                e.Handled = true; 
                return;
            }
        }


        private void TxtVencimiento_TextChanged(object sender, TextChangedEventArgs e) {
            var txt = sender as TextBox;
            if(txt == null) return;

            string text = txt.Text;

            if(text.Length == 2 && !text.Contains("/")) {
                txt.Text = text + "/";
                txt.CaretIndex = txt.Text.Length;
                return;
            }

            if(text.Length < 5)
                return;


            if(text.Length == 5) {

                if(!Regex.IsMatch(text, @"^\d{2}/\d{2}$"))
                    return;

                string mm = text.Substring(0, 2);
                string yy = text.Substring(3, 2);

                if(!int.TryParse(mm, out int mes) || mes < 1 || mes > 12) {
                    MessageBox.Show("El mes debe ser entre 01 y 12.");
                    txt.Text = "";
                    return;
                }

                if(!int.TryParse(yy, out int año) || año < 25) {
                    MessageBox.Show("El año debe ser 25 o mayor.");
                    txt.Text = $"{mm}/";
                    txt.CaretIndex = txt.Text.Length;
                    return;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void BtnClickPagar(object sender, RoutedEventArgs e) {
            string token = App.Current.Properties["Token"]?.ToString();
            int idPedido = _pedido.id;

            var servicioPago = new PagoService(new HttpClient());

            decimal monto = Convert.ToDecimal(txtTotal.Text);

            var request = new PagoRequestDTO {
                TipoPago = "Tarjeta",
                MontoPagado = monto,
                DetallesCuenta = NumeroTarjeta.Replace(" ", "") +
                                 $" | {Titular} | {Vencimiento} | CVC:{CVC}"
            };



            List<DetallesProductoDTO> productosComprados;
            var respuestaProductos = await productoPedidoService.obtenerProductosAsync(idPedido, token);
            if (respuestaProductos.productos != null && respuestaProductos.codigo == HttpStatusCode.OK)
            {
                productosComprados = respuestaProductos.productos;
                var respuesta = await productoPedidoService.comprarProductosAsync(idPedido, productosComprados, token);
                if (respuesta.codigo == HttpStatusCode.OK)
                {
                    var (pago, codigo, mensaje) =
                    await pagoService.RegistrarPagoAsync(idPedido, request, token);

                    if (codigo == HttpStatusCode.OK || codigo == HttpStatusCode.Created)
                    {
                        MessageBox.Show("Pago exitoso", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                        await GenerarTicketAsync();
                        NavigationService?.GoBack();
                    }
                    else
                    {
                        MessageBox.Show($"Error al registrar el pago:\n{mensaje}", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Uno o varios productos han agotado sus existencias", "Cantidad insuficiente", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("No se han seleccionado productos", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        public void LlenarDatosPedido() {
            txtTotal.Text = _pedido.total.ToString("0.00");
        }

        public void VaciarDatosPedido() {
            txtTotal.Text = "Total: $";
        }


        private async Task GenerarTicketAsync()
        {
            var respuesta = await ticketService.DescargarTicketAsync(_pedido.id);

            if (respuesta != null)
            {
                MessageBox.Show("Ticket descargado en" + respuesta);
            }
            else
            {
                MessageBox.Show("ERROR: ocurrió un problema al generar el ticket");
            }
        }

    }
}
