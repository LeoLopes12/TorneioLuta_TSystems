<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TorneioLuta.aspx.cs" Inherits="TorneioLuta_TSystems.TorneioLuta" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=UTF-8"/>
    <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css" />
    <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.3.1/jquery.min.js"></script>
    <script src="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/js/bootstrap.min.js"></script>
    <script type="text/javascript">
        function openModal(pMsg, pExpand) {
            $("#divMsgModal").append("<p>" + pMsg + "</p>");
            $('#myModal').modal({ show: true });
            
            if (pExpand == 'S')
                $('#modDialog').css('width', '80%');
            else
                $('#modDialog').css('width', '50%');
        }
        function mostrarOpcoes() {
            var vText = $('#mostrarOpcoes').text();

            if (vText == "Opções >>") {
                $('#divOpcoes').show();
                $('#mostrarOpcoes').text("<< Opções");
            }
            else { 
                $('#divOpcoes').hide();
                $('#mostrarOpcoes').text("Opções >>");
            }
        }
    </script>
    <title></title>
    <style>
        body {
            font-family: Calibri;
            font-size: 16px;
        }
        table{
            border-spacing: 10px;
        }
        td{
            padding: 10px;
        }
        #outer {
          text-align: center;
          width: 100%;
        }

        #inner {
          display: inline-block;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <div id="outer">
        <div id="inner" style="font-size: 28px; width: 1260px; text-align: left; font-size: 14px;">
            
            <h2 style="text-align: center;">Torneio de Luta</h2>
            
            <a id="mostrarOpcoes" onclick="javascript:mostrarOpcoes();" style="cursor: pointer; color: blue;">Opções >></a>
            <br />

            <div id="divOpcoes" style="display: none;">
                <br />
                <asp:LinkButton ID="lnkSelecionar16Pri" runat="server" style="cursor:pointer;" OnClick="lnkSelecionar16Pri_Click">Selecionar 16 primeiros</asp:LinkButton>
                <br />
                <asp:LinkButton ID="lnkSelecionar16Ale" runat="server" style="cursor:pointer;" OnClick="lnkSelecionar16Ale_Click">Selecionar 16 aleatórios</asp:LinkButton>
                <br />
                <asp:LinkButton ID="lnkLimparSelecao" runat="server" style="cursor:pointer;" OnClick="lnkLimparSelecao_Click">Limpar Seleção</asp:LinkButton>
                <br />
                <asp:CheckBox ID="chkMostrarCampeonatoComp" runat="server" Text="&nbsp;Mostrar campeonato completo" />
            </div>
            <br />

            <asp:Button ID="BtnVerificarVencedor" runat="server" Text="Iniciar Torneio" OnClick="BtnVerificarVencedor_Click" style="font-size: 16px;" />

        </div>
    </div>

    <!-- Modal -->
    <div class="modal fade" id="myModal" role="dialog">
            <div id="modDialog" class="modal-dialog" style="width: 1600px;" >

                <!-- Modal content-->
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" data-dismiss="modal">&times;</button>
                        <h4 class="modal-title">Mensagem</h4>
                    </div>
                    <div class="modal-body" id="divMsgModal">
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-primary" data-dismiss="modal">Fechar</button>
                    </div>
                </div>

            </div>
        </div>

        <asp:Panel ID="PainelLutadores" runat="server">
        </asp:Panel>
    </form>
</body>
</html>
