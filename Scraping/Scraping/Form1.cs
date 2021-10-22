using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace Scraping
{
    public partial class Form1 : Form
    {
        String url = "https://www.sears.com.mx/resultados/q=reloj/";
        int id = 110; //Numero de la pagina donde iniciara el scraping
        int x = 1; //Indice para los productos
        List<string> parts = new List<string>(); //arreglo donde almacenaremos nuestros productos
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            carga(); //metodo para cargar la pagina
        }

        public void carga()
        {
            this.webBrowser1.Url = new Uri(url + id); //Nos dezplazamos de pagina
            textBox1.Text = url + id;
            id++;
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            timer1.Start(); //Inicia el timer cuando carga la pagina
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            var links = webBrowser1.Document.GetElementsByTagName("article"); //Buscamos los elementos con el tag article (productos)
            foreach (HtmlElement link in links)
            {
                parts.Add(link.InnerHtml.ToString()); //los agregamos a nuestro arreglo
            }

            timer1.Stop();
            //MessageBox.Show("ok");
            leer();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            leer();
        }

        public void leer()
        {
            textBox1.Text = x.ToString();
            if (x < parts.Count()) //si hay productos en el array
            {
                //richTextBox1.Text = parts.ElementAt(x);
                webBrowser2.DocumentText = parts.ElementAt(x); //vamos consultando cada producto y lo colocamos en el webBrowser2
                x++;
            }
            else
            {
                parts.Clear(); // limpiamos nuestro arreglo
                x = 0;
                carga(); //cargamos la siguiente pagina
            }
        }

        private void webBrowser2_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            //richTextBox1.Text = "";
            HtmlDocument doc = webBrowser2.Document;
            //Debemos validar que hayproductos
            if(webBrowser2.Document.Body.InnerText != null && webBrowser2.Document.Body.InnerText != "Aún no tienes productos en tu Bolsa.")
            {
                String nom = "";
                String img = "";
                String precioi = "";
                String lista = "";
                nom = webBrowser2.Document.GetElementsByTagName("p")[0].InnerHtml.ToString(); //Obtenemos el nombre del producto
                try
                {
                    img = webBrowser2.Document.GetElementsByTagName("img")[0].GetAttribute("src"); //Obtenemos el link de la imagen
                }
                catch(Exception ex)
                {

                }
                //Obtenemos los precios que tenga
                if (webBrowser2.Document.GetElementsByTagName("span")[0].InnerHtml.ToString().Contains("Precio Internet:"))
                {
                    precioi = webBrowser2.Document.GetElementsByTagName("span")[1].InnerHtml.ToString();
                }
                else
                {
                    lista= webBrowser2.Document.GetElementsByTagName("span")[1].InnerHtml.ToString();
                    try
                    {
                        precioi = webBrowser2.Document.GetElementsByTagName("span")[4].InnerHtml.ToString();
                    }
                    catch(Exception ex) {
                        precioi = "0";
                    }
                }
                richTextBox1.Text += nom + "\n";
                richTextBox1.Text += img + "\n";
                richTextBox1.Text += "internet: " +precioi + "\n";
                richTextBox1.Text += "lista: " + lista + "\n";

                insert(nom.Replace("'", ""), img,lista.Replace("$","").Replace(",", ""), precioi.Replace("$", "").Replace(",", "")); //Realizamos la insercion en nuestra DB
            }
            leer();

        }

        private void insert(String nombre,String img,String lista,String internet)
        {
            String connectionline = "datasource=127.0.0.1; port=3306;username=root;password=;database=sears2;"; //127...Direccion del localhost
            MySqlConnection conn = new MySqlConnection(connectionline); //Creamos nuestra conexion con la base de datos.

            String sql = "insert into articulo values('"+nombre+"','"+img+"','"+lista+"','"+internet+"')"; //llamamos nuestra consulta

            MySqlCommand command = new MySqlCommand(sql,conn);

            command.Connection.Open();
            command.ExecuteNonQuery(); //Corre la consulta


            conn.Close();
        }
    }
}
