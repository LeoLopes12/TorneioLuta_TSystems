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
            
            CheckBox chkLutador = new CheckBox();
            chkLutador.Text = "&nbsp;&nbsp;" + HttpUtility.HtmlDecode(pLutador.nome);
            chkLutador.ID = "chk_" + pLutador.id;

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
            
            return table;

        }

        protected void BtnVerificarVencedor_Click(object sender, EventArgs e)
        {

            string[] vLutadoresSel = GetSelecionados().Split(',');
            if (vLutadoresSel.Length != 16)
                AlertaModal("Por favor, é necessário que sejam selecionados exatamente 16 lutadores.", "N");
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
            
            // ***** SEMI - FIM ***** //

            // ***** FINAL - INICIO ***** //

                vEstagioCamp = 4; //Estágio campeonato = 4 -- Significa que está na final
                RealizarLuta();
                if (vInterromperCompeticao == true)
                    return;

            // ***** FINAL - FIM ***** //

            string tbResFinal = "" +
                "<table align=\"center\" style=\"text-align: center; font-size: 20px; \"> " +
                "   <tr>" +
                "       <td>Campeão</td>" +
                "   </tr>" +
                "   <tr>" +
                "       <td><b>" + resLutas_Final[0].nomeVencedor + "</b></td>" +
                "   </tr>" +
                "</table>";

            string vExpand = "N";
            if (chkMostrarCampeonatoComp.Checked) { 
                tbResFinal = GetDadosCompeticao() + "<br>" + tbResFinal;
                vExpand = "S";
            }

            AlertaModal(tbResFinal, vExpand);

        }

       
        void AlertaModal(string pMsg, string vExpand)
        {
            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "myModal", "openModal('" + pMsg + "', '" + vExpand + "');", true);
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

        bool DadoIncluso(int pIdLutador, string pIds) {

            if (pIds == "")
                return false;
            else
                pIds = pIds.Substring(0, pIds.Length - 1); //Remove última virgula da variavel...

            //Verifica se o lutador em questão já foi incluso em lutas anteriores.
            //  ... Método responsável por não repetir lutadores ao buscar o mais jovem.
            string[] vArr = pIds.Split(',');
            for (int x = 0; x < vArr.Length; x++) {
                if (pIdLutador == Convert.ToInt32(vArr[x]))
                    return true;
            }

            return false;
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
            
                if (lutador.idade < idadeMin && DadoIncluso(lutador.id, vExc) == false)
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
                                                GetNome(pComp1) + " e " + GetNome(pComp2), "N");
                                    return -1;

                                }

                            }
                        }

                    }

                }
            }
            
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

        protected void lnkSelecionar16Pri_Click(object sender, EventArgs e)
        {
            LimparCheckBoxes();

            int x = 0;
            //Realiza um loop em cada um dos lutadores
            foreach (var lutador in resultados)
            {
                
                CheckBox chkLutador = (CheckBox)Page.FindControl("chk_" + lutador.id);
                chkLutador.Checked = true;
                x++;

                if (x == 16) 
                    return;
   
            }
            
        }

        int GetMenorId() {
            int menor = -1;
            foreach (var lutador in resultados)
            {
                if (menor == -1)
                    menor = lutador.id; //Somente inializando a variavel 
                else 
                    if (lutador.id < menor)
                        menor = lutador.id;
            }
            return menor;
        }

        int GetMaiorId() {
            int maior = -1;
            foreach (var lutador in resultados)
            {
                if (maior == -1)
                    maior = lutador.id; //Somente inializando a variavel 
                else
                    if (lutador.id > maior)
                        maior = lutador.id;
            }
            return maior;

        }

        protected void lnkSelecionar16Ale_Click(object sender, EventArgs e)
        {
            LimparCheckBoxes();

            int x = 0;
            string vInclusos = "";

            int vMenorId = GetMenorId();
            int vMaiorId = GetMaiorId();
            
            //Realiza um loop em cada um dos lutadores
            foreach (var lutador in resultados)
            {

                int vIdRandom = GetId_Aleatorio(vInclusos, vMenorId, vMaiorId);
                
                CheckBox chkLutador = (CheckBox)Page.FindControl("chk_" + vIdRandom);
                chkLutador.Checked = true;

                vInclusos += vIdRandom + ",";
                x++;

                if (x == 16) 
                    return;
   
            }
            
        }

        int GetId_Aleatorio(string pInclusos, int pMenorId, int pMaiorId) {
            
            int vNumRandom;

            do
            {
                Random randNum = new Random();
                vNumRandom = randNum.Next(pMenorId, pMaiorId);
              
            }
            while (DadoIncluso(vNumRandom, pInclusos)); //Repetir o processo se o Id já estiver sido sorteado antes...

            return vNumRandom;

        }

        protected void lnkLimparSelecao_Click(object sender, EventArgs e)
        {
            LimparCheckBoxes();
        }

        void LimparCheckBoxes(){
            //Realiza um loop em cada um dos lutadores
            foreach (var lutador in resultados)
            {
                CheckBox chkLutador = (CheckBox)Page.FindControl("chk_" + lutador.id);
                if (chkLutador.Checked == true)
                    chkLutador.Checked = false;
            }
        }

        string GetDadosCompeticao()
        {

            string tbGeral = "";
            var resLutas = resLutas_Oitavas;
            var titulo = "Oitavas";

            for (int i = 1; i <= 4; i++)
            {

                switch (i)
                {
                    case 2:
                        resLutas = resLutas_Quartas;
                        titulo = "Quartas";
                        break;
                    case 3:
                        resLutas = resLutas_Semi;
                        titulo = "Semi";
                        break;
                    case 4:
                        resLutas = resLutas_Final;
                        titulo = "Final";
                        break;
                }

                int vColSpan = Convert.ToInt32(resLutas.Count()) + 1;
                string tbDados = "<table align=\"center\" style=\"text-align: center; font-size: 14px; border: 1px solid silver; \">";
                tbDados += "<tr>" +
                           "    <td  style=\"text-align: center; font-size: 22px; \" colspan=" + vColSpan + "> " + titulo + " </td>" +
                           "</tr>";

                int x = 1;
                for (x = 1; x <= 2; x++)
                {

                    string vTitulo = "Combate";
                    if (x > 1)
                        if (i < 4)
                            vTitulo = "Vencedor";
                        else
                            vTitulo = "";

                    tbDados += "<tr><td><b>" + vTitulo + "</b></td>";

                    foreach (var lutas in resLutas)
                    {
                        string vConteudo = "";
                        if (x == 1)
                            vConteudo = lutas.nomeLutador1 + "  <br>VS<br>" + lutas.nomeLutador2;
                        else
                            if (i < 4)
                            vConteudo = lutas.nomeVencedor;

                        tbDados += "<td align=\"center\" > " + vConteudo + "</td>";
                    }
                    tbDados += "</tr>";
                }
                tbDados += "</table>";

                tbGeral += tbDados + "<br>";
            }

            return tbGeral;
        }

    }

}