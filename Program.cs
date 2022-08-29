using SecurityProgram.Enums;
using SecurityProgram.Funcoes;
using System.Text.Json;

namespace SecurityProgram
{
    public class Program
    {
        static void Main(string[] args)
        {
            try
            {
                MenuUsuario.ImprimirMensagem("Carregando...");
                SegurancaSistema.CarregarConfiguracao();

                OpcoesUsuario enumOpcoes = OpcoesUsuario.Fechar;

                do
                {
                    if (Usuarios.ValidaLoginUsuario(true))
                    {
                        enumOpcoes = MenuUsuario.ReceberOpcaoUsuario();

                        switch (enumOpcoes)
                        {
                            case OpcoesUsuario.CadastrarUsuário:
                                Usuarios.CadastrarUsuario();
                                break;

                            case OpcoesUsuario.Login:
                                Usuarios.Login();
                                break;

                            case OpcoesUsuario.Fechar:
                                return;

                            default:
                                MenuUsuario.ImprimirErro("Opção Indisponível, tente novamente.");
                                break;
                        }
                    }
                    else
                    {
                        OpcoesUsuarioLogado enumOpcoesUsuarioLogado = MenuUsuario.ReceberOpcaoUsuarioLogado();

                        switch (enumOpcoesUsuarioLogado)
                        {
                            case OpcoesUsuarioLogado.CadastrarDocumento:
                                Usuarios.CadastrarDocumento();
                                break;

                            case OpcoesUsuarioLogado.ListarDocumentos:
                                Usuarios.ListarAtributosDocumentosUsuario();
                                break;

                            case OpcoesUsuarioLogado.ListarDocumentosAssinados:
                                Usuarios.ListarDocumentosAssinados();
                                break;

                            case OpcoesUsuarioLogado.AssinarDocumento:
                                Usuarios.AssinarDocumento();
                                break;

                            case OpcoesUsuarioLogado.DeletarDocumento:
                                Usuarios.DeletarDocumento();
                                break;

                            case OpcoesUsuarioLogado.VerificarAssinatura:
                                Usuarios.VerificarAssinatura();
                                break;


                            case OpcoesUsuarioLogado.AtualizarSenha:
                                Usuarios.AlterarSenha();
                                break;


                            case OpcoesUsuarioLogado.DeletarConta:
                                Usuarios.Remover();
                                break;

                            case OpcoesUsuarioLogado.Sair:
                                Usuarios.Logout();
                                break;

                            default:
                                MenuUsuario.ImprimirErro("Informe uma opção válida, tente novamente.");
                                MenuUsuario.Continuar();
                                break;
                        }
                    }
                } while (enumOpcoes != OpcoesUsuario.Fechar);
            }
            catch (ArgumentNullException ex)
            {
                MenuUsuario.ImprimirErro(ex.Message);
            }
            catch (ArgumentException ex)
            {
                MenuUsuario.ImprimirErro(ex.Message);
            }
            catch (JsonException ex)
            {
                MenuUsuario.ImprimirErro(ex.Message);
            }
            catch (Exception)
            {
                MenuUsuario.ImprimirErro("Ocorreu um erro na aplicação.");
            }
            finally
            {
                try
                {
                    BancoDadosSistema.Dispose();
                    MenuUsuario.Continuar("\nAperte uma tecla para finalizar a aplicação.");
                }
                catch (Exception)
                {
                    MenuUsuario.Continuar("\nAperte uma tecla para finalizar a aplicação.");
                }
            }
        }
    }
}