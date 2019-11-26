using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using System.Text;

namespace TorneioLuta_TSystems
{

    public partial class TorneioLuta : System.Web.UI.Page
    {
        //Resultados gerais de todos lutadores - Obtido através do JSon
        List<Lutador> resultados = new List<Lutador>();

        List<Lutador> vLutadoresSelecionados_Oitavas = new List<Lutador>(15);
        List<Lutador> vLutadoresSelecionados_Quartas = new List<Lutador>(7);

        List<Lutas> resLutas_Oitavas = new List<Lutas>();
        List<Lutas> resLutas_Quartas = new List<Lutas>();
        List<Lutas> resLutas_Semi = new List<Lutas>();
        List<Lutas> resLutas_Final = new List<Lutas>();

        int vEstagioCamp = 0;
        string vCompetidoresAdd = "";

        bool vInterromperCompeticao = false;

        protected void Page_Load(object sender, EventArgs e)
        {
            CriarObjetos();
        }

        void CriarObjetos()
        {

            WebClient WC = new WebClient();
            WC.Encoding = Encoding.UTF8;
            var json = WC.DownloadString("http://177.36.237.87/lutadores/api/competidores");

            //Cria uma lista com os dados dos lutadores contidos no json
            resultados = JsonConvert.DeserializeObject<List<Lutador>>(json);

            Table tbMae = new Table();
            tbMae.CellSpacing = 50;
            tbMae.HorizontalAlign = HorizontalAlign.Center;
            TableRow tbRowMae = new TableRow();

            //Variavel somente para controlar a quantidade de lutadores por linha
            int cont = 0;
            int contGeral = 0;

            //Realiza um loop em cada um dos lutadores
            foreach (var lutador in resultados)
            {

                //Se já foram adicionados 8 quadros, criaremos outra linha da tabela Mãe
                if (cont == 4)
                {
                    tbRowMae = new TableRow();

                    //Retornamos o contador para 1 para reiniciar o procedimento
                    cont = 0;
                }

                //Cria a tabela completa com os dados do lutador especifico
                Table tbLutador = CriarTabelaLutador(lutador, contGeral);

                TableCell tbCellMae = new TableCell();
                tbCellMae.Controls.Add(tbLutador);

                tbRowMae.Cells.Add(tbCellMae);

                tbMae.Rows.Add(tbRowMae);

                cont++;
                contGeral++;

            }

            //Adiciona a tabela ao painel do WebForm
            PainelLutadores.Controls.Add(tbMae);

        }

        Table CriarTabelaLutador(Lutador pLutador, int cont)
        {

            Table table = new Table();
            table.Width = 300;
            table.BorderColor = System.Drawing.Color.Black;
            table.BorderStyle = System.Web.UI.WebControls.BorderStyle.Solid;
            table.BorderWidth = 1;

            bool vChecked = false;

            if (cont <= 15)
                vChecked = true;

            CheckBox chkLutador = new CheckBox();
            chkLutador.Text = "&nbsp;&nbsp;" + HttpUtility.HtmlDecode(pLutador.nome);
            chkLutador.ID = "chk_" + pLutador.id;
            chkLutador.Checked = vChecked;

            // ***** Adiciona Nome ***** //
            TableCell cellNome = new TableCell();
            cellNome.BackColor = System.Drawing.Color.LightBlue;
            cellNome.HorizontalAlign = HorizontalAlign.Center;
            cellNome.Controls.Add(chkLutador);

            TableRow rowNome = new TableRow();
            rowNome.Cells.Add(cellNome);
            table.Rows.Add(rowNome);

            // ***** Adiciona demais dados ***** //
            TableCell cellDados = new TableCell();

            string[] artesMarciais = pLutador.artesMarciais;

            string vConteudo = "Idade: " + pLutador.idade + "<br>";
            vConteudo += "Lutas: " + pLutador.lutas + "<br>";
            vConteudo += "Derrotas: " + pLutador.derrotas + "<br>";
            vConteudo += "Vitórias: " + pLutador.vitorias + "<br>";
            vConteudo += "Artes Marciais: " + artesMarciais.Length + "<br>";
            cellDados.Text = vConteudo;

            TableRow rowDados = new TableRow();
            rowDados.Cells.Add(cellDados);

            table.Rows.Add(rowDados);

            /*
                // ***** Adiciona Nome ***** //
                TableCell cellNome = new TableCell();
                cellNome.BackColor = System.Drawing.Color.LightBlue;
                cellNome.HorizontalAlign = HorizontalAlign.Center;
                cellNome.Controls.Add(chkLutador);
            
                TableRow rowNome = new TableRow();
                rowNome.Cells.Add(cellNome);

                table.Rows.Add(rowNome);

                // ***** Adiciona Idade ***** //
                TableCell cellIdade = new TableCell();
                cellIdade.Text = "Idade: " + pLutador.idade;

                TableRow rowIdade = new TableRow();
                rowIdade.Cells.Add(cellIdade);

                table.Rows.Add(rowIdade);
            */

            return table;

        }

        public class Lutador
        {

            public int id { get; set; }
            public string nome { get; set; }
            public int idade { get; set; }
            public string[] artesMarciais { get; set; }
            public int lutas { get; set; }
            public int derrotas { get; set; }
            public int vitorias { get; set; }

        }

        public class Lutas
        {
            public String nomeLutador1 { get; set; }
            public String nomeLutador2 { get; set; }
            public int idVencedor { get; set; }
            public String nomeVencedor { get; set; }
        }

        protected void BtnVerificarVencedor_Click(object sender, EventArgs e)
        {

            string[] vLutadoresSel = GetSelecionados().Split(',');
            if (vLutadoresSel.Length != 16)
                AlertaModal("Por favor, é necessário que sejam selecionados exatamente 16 lutadores.");
            else
            {

                //Cria uma list 'enxuta' - Somente dos lutadores selecionados
                SetarDadosLutadoresSelecionados(vLutadoresSel);

                //Método responsável por realizar o campeonato - desde as oitavas até a final
                RealizarCampeonato();
            }

        }

        void SetarDadosLutadoresSelecionados(string[] pLutadoresSel)
        {

            /*
              Gera list com os dados (somente dos lutadores selecionados)
              ...Somente para ter uma lista mais enxuta no momento das comparações...
            */

            for (int x = 0; x <= pLutadoresSel.Length - 1; x++)
            {

                //Retorna dados do lutador de Id que está sendo passado no parâmetro...
                Lutador vLut = GetDadosLutador(Convert.ToInt32(pLutadoresSel[x]));
                vLutadoresSelecionados_Oitavas.Add(vLut);

            }

            //AlertaModal(vLutadoresSelecionados_Oitavas[1].nome);

        }

        Lutador GetDadosLutador(int pId)
        {

            Lutador vRetLutador = new Lutador();

            //Realiza um loop em cada um dos lutadores
            foreach (var lutador in resultados)
            {
                if (lutador.id == pId)
                    vRetLutador = lutador;

            }

            return vRetLutador;
        }

        void RealizarCampeonato()
        {

            // ***** OITAVAS - INICIO ***** //

            //Estágio campeonato = 1 -- Significa que está nas oitavas
            vEstagioCamp = 1;

            //Resolver as oitavas...
            for (int x = 1; x <= 8; x++)
            {
                RealizarLuta();
                if (vInterromperCompeticao == true)
                    return;
            }
            Response.Write("Oitavas<br>");

            //Realiza um loop em cada um dos lutadores
            foreach (var lutas in resLutas_Oitavas)
            {
                Response.Write(lutas.nomeLutador1 + " VS " + lutas.nomeLutador2 + " = " + lutas.nomeVencedor + "<br>");
            }

            // ***** OITAVAS - FIM ***** //

            // ---------- ---------- ---------- ---------- //

            // ***** QUARTAS - INICIO ***** //

            vEstagioCamp = 2; //Estágio campeonato = 2 -- Significa que está nas quartas
            vCompetidoresAdd = "";

            //Resolver as quartas...
            for (int x = 1; x <= 4; x++)
            {
                RealizarLuta();
                if (vInterromperCompeticao == true)
                    return;
            }
            Response.Write("<br>Quartas<br>");

            //Realiza um loop em cada um dos lutadores
            foreach (var lutas in resLutas_Quartas)
            {
                Response.Write(lutas.nomeLutador1 + " VS " + lutas.nomeLutador2 + " = " + lutas.nomeVencedor + "<br>");
            }

            // ***** QUARTAS - FIM ***** //

            // ---------- ---------- ---------- ---------- //

            // ***** SEMI - INICIO ***** //

            vEstagioCamp = 3; //Estágio campeonato = 3 -- Significa que está na semi final
            for (int x = 0; x <= 3; x += 2)
            {
                RealizarLuta(x);
                if (vInterromperCompeticao == true)
                    return;
            }

            Response.Write("<br>Semi<br>");

            //Realiza um loop em cada um dos lutadores
            foreach (var lutas in resLutas_Semi)
            {
                Response.Write(lutas.nomeLutador1 + " VS " + lutas.nomeLutador2 + " = " + lutas.nomeVencedor + "<br>");
            }

            // ***** SEMI - FIM ***** //

            // ***** FINAL - INICIO ***** //

            vEstagioCamp = 4; //Estágio campeonato = 4 -- Significa que está na final
            RealizarLuta();
            if (vInterromperCompeticao == true)
                return;

            Response.Write("<br>Final<br>");
            Response.Write(resLutas_Final[0].nomeLutador1 + " VS " + resLutas_Final[0].nomeLutador2 + " = " + resLutas_Final[0].nomeVencedor + "<br>");

            // ***** FINAL - FIM ***** //

            string tbVencedor = "" +
                "<table align=\"center\" style=\"text-align: center; font-size: 20px; \"> " +
                "   <tr>" +
                "       <td>Campeão</td>" +
                "   </tr>" +
                "   <tr>" +
                "       <td><b>" + resLutas_Final[0].nomeVencedor + "</b></td>" +
                "   </tr>" +
                "</table>";

            AlertaModal(tbVencedor);

        }

        void AlertaModal(string pMsg)
        {
            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "myModal", "openModal('" + pMsg + "');", true);
        }

        string GetSelecionados()
        {

            string vSelecionados = "";

            //Realiza um loop em cada um dos lutadores
            foreach (var lutador in resultados)
            {
                CheckBox chkLutador = (CheckBox)Page.FindControl("chk_" + lutador.id);
                if (chkLutador.Checked == true)
                {
                    vSelecionados += lutador.id + ",";
                }
            }

            if (vSelecionados != "")
                vSelecionados = vSelecionados.Substring(0, vSelecionados.Length - 1); //Remove última ","

            return vSelecionados;
        }

        int GetCompetidorMaisJovem(string vExc)
        {
            //Esse método retorna sempre o próximo competidor mais jovem
            //  ... A variavel vExc serve para controle - Para não adicionar repetido

            int idadeMin = 200;
            int idCompetidor = 0;

            var vLutadores = vLutadoresSelecionados_Oitavas;

            if (vEstagioCamp == 2)
                vLutadores = vLutadoresSelecionados_Quartas;

            //Realiza um loop em cada um dos lutadores
            foreach (var lutador in vLutadores)
            {

                if (lutador.idade < idadeMin && vExc.Contains(Convert.ToString(lutador.id)) == false)
                {
                    idCompetidor = lutador.id;
                    idadeMin = lutador.idade;
                }

            }

            return idCompetidor;
        }

        void RealizarLuta(int pIndex = -1)
        {

            int vCompetidor1 = 0;
            int vCompetidor2 = 0;

            if (vEstagioCamp <= 2)
            {

                //Resgata os competidores para as oitavas e quartas... 

                vCompetidor1 = GetCompetidorMaisJovem(vCompetidoresAdd);
                vCompetidoresAdd += vCompetidor1 + ",";

                vCompetidor2 = GetCompetidorMaisJovem(vCompetidoresAdd);
                vCompetidoresAdd += vCompetidor2 + ",";

            }
            else
            {

                //Restata os competidores para a semi e a final...

                if (vEstagioCamp == 3)
                {

                    //Semifinal 
                    vCompetidor1 = resLutas_Quartas[pIndex].idVencedor;
                    vCompetidor2 = resLutas_Quartas[pIndex + 1].idVencedor;

                }
                else
                {

                    //Final
                    vCompetidor1 = resLutas_Semi[0].idVencedor;
                    vCompetidor2 = resLutas_Semi[1].idVencedor;

                }

            }

            int vVencedor = GetVencedor(vCompetidor1, vCompetidor2);

            switch (vEstagioCamp)
            {
                case 1:

                    //Quando cair aqui, obrigatóriamente estará nas oitavas ... 
                    resLutas_Oitavas.Add(new TorneioLuta_TSystems.TorneioLuta.Lutas()
                    {
                        nomeLutador1 = GetNome(vCompetidor1),
                        nomeLutador2 = GetNome(vCompetidor2),
                        idVencedor = vVencedor,
                        nomeVencedor = GetNome(vVencedor)
                    }
                    );
                    vLutadoresSelecionados_Quartas.Add(GetDadosLutador(vVencedor));
                    break;

                case 2:

                    //Quando cair aqui, obrigatóriamente estará nas quartas ... 
                    resLutas_Quartas.Add(new TorneioLuta_TSystems.TorneioLuta.Lutas()
                    {
                        nomeLutador1 = GetNome(vCompetidor1),
                        nomeLutador2 = GetNome(vCompetidor2),
                        idVencedor = vVencedor,
                        nomeVencedor = GetNome(vVencedor)
                    }
                    );
                    break;

                case 3:

                    //Quando cair aqui, obrigatóriamente estará na semi ... 
                    resLutas_Semi.Add(new TorneioLuta_TSystems.TorneioLuta.Lutas()
                    {
                        nomeLutador1 = GetNome(vCompetidor1),
                        nomeLutador2 = GetNome(vCompetidor2),
                        idVencedor = vVencedor,
                        nomeVencedor = GetNome(vVencedor)
                    }
                    );
                    break;

                case 4:

                    //Quando cair aqui, obrigatóriamente estará naa final ... 
                    resLutas_Final.Add(new TorneioLuta_TSystems.TorneioLuta.Lutas()
                    {
                        nomeLutador1 = GetNome(vCompetidor1),
                        nomeLutador2 = GetNome(vCompetidor2),
                        idVencedor = vVencedor,
                        nomeVencedor = GetNome(vVencedor)
                    }
                    );
                    break;
            }


        }

        int GetVencedor(int pComp1, int pComp2)
        {

            int vPorcVitorias_Comp1 = GetPorcVitoria(pComp1);
            int vPorcVitorias_Comp2 = GetPorcVitoria(pComp2);

            if (vPorcVitorias_Comp1 > vPorcVitorias_Comp2)
                return pComp1; //Retorna vencedor (Competidor 1)
            else
            {

                if (vPorcVitorias_Comp2 > vPorcVitorias_Comp1)
                    return pComp2; //Retorna vencedor (Competidor 2)
                else
                {

                    // Vai cair aqui sempre que houver impate...    

                    /*
                        1.  Primeiramente será comparado a quantidade de artes marciais;
                        2.  Caso a Qtd. de Artes Marciais não seja o suficiente, será comparado o número de lutas... 
                    */

                    int vTotalArtesMarciais_Comp1 = GetTotalArtesMarciais(pComp1);
                    int vTotalArtesMarciais_Comp2 = GetTotalArtesMarciais(pComp2);

                    if (vTotalArtesMarciais_Comp1 > vTotalArtesMarciais_Comp2)
                        return pComp1;
                    else
                    {

                        if (vTotalArtesMarciais_Comp2 > vTotalArtesMarciais_Comp1)
                            return pComp2;
                        else
                        {

                            //Aqui será comparado o número de lutas (Segundo critério de desempate)

                            int vTotalLutas_Comp1 = GetTotalLutas(pComp1);
                            int vTotalLutas_Comp2 = GetTotalLutas(pComp2);

                            if (vTotalLutas_Comp1 > vTotalLutas_Comp2)
                                return pComp1;
                            else
                            {

                                if (vTotalLutas_Comp2 > vTotalLutas_Comp1)
                                    return pComp2;
                                else
                                {

                                    //Somente por segurança, caso ainda exista um empate no último critério de desempate...
                                    vInterromperCompeticao = true;
                                    AlertaModal("Não foi possivel concluir o torneio!" +
                                                "<br>Motivo: Houve um empate entre os lutadores " +
                                                GetNome(pComp1) + " e " + GetNome(pComp2));
                                    return -1;

                                }

                            }
                        }

                    }

                }
            }

            //Response.Write(GetNome(pComp1) + " -- " + vPorcVitorias_Comp1 + "<br>");

        }

        string GetNome(int pId)
        {

            string vNome = "Indefinido";

            //Realiza um loop em cada um dos lutadores
            foreach (var lutador in vLutadoresSelecionados_Oitavas)
            {

                if (lutador.id == pId)
                {
                    vNome = lutador.nome;
                    break;
                }

            }

            return vNome;
        }

        int GetTotalArtesMarciais(int pId)
        {

            int vQtdArtesMarciais = 0;

            //Realiza um loop em cada um dos lutadores
            foreach (var lutador in vLutadoresSelecionados_Oitavas)
            {

                if (lutador.id == pId)
                {
                    vQtdArtesMarciais = lutador.artesMarciais.Length;
                    break;
                }

            }

            return vQtdArtesMarciais;
        }

        int GetTotalLutas(int pId)
        {

            int vQtdLutas = 0;

            //Realiza um loop em cada um dos lutadores
            foreach (var lutador in vLutadoresSelecionados_Oitavas)
            {

                if (lutador.id == pId)
                {
                    vQtdLutas = lutador.lutas;
                    break;
                }

            }

            return vQtdLutas;
        }


        int GetPorcVitoria(int pId)
        {
            int vRet = 0;

            //Realiza um loop em cada um dos lutadores
            foreach (var lutador in vLutadoresSelecionados_Oitavas)
            {

                if (lutador.id == pId)
                {
                    vRet = Convert.ToInt32(Math.Round(Convert.ToDouble(lutador.vitorias) / Convert.ToDouble(lutador.lutas) * 100, 2));
                    break;
                }

            }

            return vRet;
        }

    }
}