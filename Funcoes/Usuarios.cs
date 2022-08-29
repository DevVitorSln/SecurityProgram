using SecurityProgram.Enums;
using SecurityProgram.Models;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace SecurityProgram.Funcoes
{
    public static class Usuarios
    {
        private static Usuario _usuario = null;

        #region .:Usuarios:
        public static IEnumerable<Usuario> ListarNomesAssinantes()
        {
            var listaAssinantes = BancoDadosSistema.CarregarAssinantes(_usuario);

            if (listaAssinantes == null || listaAssinantes.Count() == 0)
            {
                return null;
            }
            else
            {
                string imprirmirLista = ConstruirListaAssinantes(listaAssinantes);

                MenuUsuario.ImprimirMensagem(imprirmirLista);

                return listaAssinantes;
            }
        }

        public static void CadastrarUsuario()
        {
            MenuUsuario.ImprimirMensagem("\n|| CADASTRO DE USUÁRIO ||\n\nNome: ");

            string nome = MenuUsuario.ReceberValorInserido();

            if (string.IsNullOrWhiteSpace(nome))
            {
                MenuUsuario.ImprimirErro("O nome do usuário deve ser informado, tente novamente.");
                MenuUsuario.Continuar();

                return;
            }
            else
            {
                var verificaUsuario = BancoDadosSistema.VerificarUsuario(nome);

                if (verificaUsuario)
                {
                    MenuUsuario.ImprimirErro("Usuário já cadastrado, tente novamente.");
                    MenuUsuario.Continuar();

                    return;
                }
                else
                {
                    MenuUsuario.ImprimirMensagem("\nSenha: ");
                    var senha = SegurancaSistema.MascararSenha();
                    string rawPassword = new NetworkCredential(string.Empty, senha).Password;

                    if (string.IsNullOrWhiteSpace(rawPassword))
                    {
                        MenuUsuario.ImprimirErro("A senha do usuário deve ser informada, tente novamente.");
                        MenuUsuario.Continuar();

                        return;
                    }
                    else
                    {
                        if (ValidarSenha(rawPassword))
                        {
                            MenuUsuario.ImprimirMensagem("\nConfirme a Senha: ");
                            var confirmaSenha = SegurancaSistema.MascararSenha();
                            string rawPasswordConfirm = new NetworkCredential(string.Empty, confirmaSenha).Password;

                            if (string.IsNullOrWhiteSpace(rawPasswordConfirm))
                            {
                                MenuUsuario.ImprimirErro("Confirme a senha para realizar o cadastro, tente novamente.");
                                MenuUsuario.Continuar();

                                return;
                            }
                            else
                            {
                                if (rawPassword.Equals(rawPasswordConfirm, StringComparison.InvariantCulture))
                                {
                                    string hashSenha = SegurancaSistema.GerarHash(rawPasswordConfirm);

                                    RSA parDeChaves = SegurancaSistema.GeraParDeChaves();

                                    byte[] chavePublica = parDeChaves.ExportRSAPublicKey();
                                    byte[] chavePrivada = parDeChaves.ExportRSAPrivateKey();

                                    string chavePublicaB64Encode = Convert.ToBase64String(chavePublica);
                                    string chavePrivadaB64Encode = Convert.ToBase64String(chavePrivada);

                                    var md5 = MD5.Create();
                                    var hash = md5.ComputeHash(Encoding.Default.GetBytes(nome));
                                    Guid id = new Guid(hash);

                                    Usuario usuario = new Usuario(nome, hashSenha, chavePublicaB64Encode, chavePrivadaB64Encode, id);
                                    BancoDadosSistema.InserirUsuario(usuario);

                                    MenuUsuario.ImprimirMensagem("\nUsuário cadastrado com sucesso!");
                                    MenuUsuario.Continuar();

                                    return;

                                }
                                else
                                {
                                    MenuUsuario.ImprimirErro("Senhas não coincidem, tente novamente.");
                                    MenuUsuario.Continuar();

                                    return;
                                }
                            }
                        }
                        else
                        {
                            MenuUsuario.ImprimirErro("a senha deve conter no minímo 8 caracteres e pelo menos um(a):\n-Número.\n-Letra Maiúscula.\n-Caracter Especial.\n\nTente Novamente.");
                            MenuUsuario.Continuar();
                        }
                    }
                }
            }
        }

        public static void Login()
        {
            MenuUsuario.ImprimirMensagem("\n|| LOGIN ||\n\nNome: ");

            string nome = MenuUsuario.ReceberValorInserido();

            if (string.IsNullOrWhiteSpace(nome))
            {
                MenuUsuario.ImprimirErro("O nome do usuário deve ser informado, tente novamente.");
                MenuUsuario.Continuar();

                return;
            }
            else
            {
                string mensagemSenha = "\nSenha: ";
                MenuUsuario.ImprimirMensagem(mensagemSenha);

                var senha = SegurancaSistema.MascararSenha();
                string rawPassword = new NetworkCredential(string.Empty, senha).Password;

                if (string.IsNullOrWhiteSpace(rawPassword))
                {
                    MenuUsuario.ImprimirErro("A senha do usuário deve ser informada, tente novamente.");
                    MenuUsuario.Continuar();
                }
                else
                {
                    var hashSenha = SegurancaSistema.GerarHash(rawPassword);
                    _usuario = BancoDadosSistema.RetornarUsuario(nome, hashSenha);

                    if (_usuario != null)
                    {
                        MenuUsuario.ImprimirMensagem("\nLogin efetuado com sucesso!");
                        MenuUsuario.Continuar("\nAperte uma tecla para iniciar sessão.");

                        return;
                    }
                    else
                    {
                        MenuUsuario.ImprimirErro("Credenciais inválidas, tente novamente.");
                        MenuUsuario.Continuar();

                        return;
                    }
                }
            }
        }
        public static void Logout()
        {
            if (_usuario != null)
                _usuario = null;
        }
        public static bool ValidaLoginUsuario(bool suprimirErro = false)
        {
            if (!suprimirErro && _usuario == null)
            {
                throw new ArgumentNullException("\nChame este metódo somente quando o usuário estiver logado.");
            }
            else
            {
                return _usuario == null;
            }
        }

        public static void AlterarSenha()
        {
            ValidaLoginUsuario();

            MenuUsuario.ImprimirMensagem("\n|| ATUALIZAÇÃO DE SENHA ||\n\nInforme senha atual: ");

            var senhaAtual = SegurancaSistema.MascararSenha();
            string rawPassword = new NetworkCredential(string.Empty, senhaAtual).Password;

            if (string.IsNullOrWhiteSpace(rawPassword))
            {
                MenuUsuario.ImprimirErro("A senha atual do usuário deve ser informada, tente novamente.");
                MenuUsuario.Continuar();

                return;
            }

            string hashSenha = SegurancaSistema.GerarHash(rawPassword);

            if (hashSenha.Equals(_usuario.Senha, StringComparison.InvariantCulture))
            {
                MenuUsuario.ImprimirMensagem("\nInforme a nova Senha: ");

                var senhaAux = SegurancaSistema.MascararSenha();
                string rawPasswordAux = new NetworkCredential(string.Empty, senhaAux).Password;

                if (string.IsNullOrWhiteSpace(rawPasswordAux))
                {
                    MenuUsuario.ImprimirErro("A nova senha do usuário deve ser informada, tente novamente.");
                    MenuUsuario.Continuar();

                    return;
                }
                else
                {
                    if (ValidarSenha(rawPasswordAux))
                    {
                        if (rawPassword.Equals(rawPasswordAux, StringComparison.InvariantCulture))
                        {
                            MenuUsuario.ImprimirErro("A senha do usuário deve ser diferente da atual, tente novamente.");
                            MenuUsuario.Continuar();

                            return;
                        }
                        else
                        {
                            MenuUsuario.ImprimirMensagem("\n\nConfirme a nova senha: ");

                            var senhaAuxConfirm = SegurancaSistema.MascararSenha();
                            string rawPasswordAuxConfirm = new NetworkCredential(string.Empty, senhaAuxConfirm).Password;


                            if (string.IsNullOrWhiteSpace(rawPasswordAuxConfirm))
                            {
                                MenuUsuario.ImprimirErro("Informe a confirmação da senha para realizar a atualização, tente novamente.");
                                MenuUsuario.Continuar();

                                return;
                            }
                            else
                            {
                                if (rawPasswordAux.Equals(rawPasswordAuxConfirm, StringComparison.InvariantCulture))
                                {
                                    string hashSenhaAuxConfirm = SegurancaSistema.GerarHash(rawPasswordAuxConfirm);

                                    _usuario.Senha = hashSenhaAuxConfirm;
                                    BancoDadosSistema.AtualizarUsuario(_usuario);

                                    MenuUsuario.ImprimirMensagem("\nSenha atualizada com sucesso!");
                                    MenuUsuario.Continuar();

                                    return;

                                }
                                else
                                {
                                    MenuUsuario.ImprimirErro("Senhas não coincidem, tente novamente.");
                                    MenuUsuario.Continuar();

                                    return;
                                }
                            }
                        }
                    }
                    else
                    {
                        MenuUsuario.ImprimirErro("a senha deve conter no minímo 8 caracteres e pelo menos um(a):\n-Número.\n-Letra Maiúscula.\n-Caracter Especial.\n\nTente Novamente.");
                        MenuUsuario.Continuar();
                    }
                }
            }
            else
            {
                MenuUsuario.ImprimirErro("Credenciais inválidas, tente novamente.");
                MenuUsuario.Continuar();

                return;
            }
        }

        public static void Remover()
        {
            ValidaLoginUsuario();

            MenuUsuario.ImprimirMensagem("\n|| DELETAR CONTA ||\n\nSenha: ");

            var senhaAtual = SegurancaSistema.MascararSenha();
            string rawPassword = new NetworkCredential(string.Empty, senhaAtual).Password;

            if (string.IsNullOrWhiteSpace(rawPassword))
            {
                MenuUsuario.ImprimirErro("A senha do usuário deve ser informada, tente novamente.");
                MenuUsuario.Continuar();

                return;
            }

            string hashSenha = SegurancaSistema.GerarHash(rawPassword);

            if (hashSenha.Equals(_usuario.Senha, StringComparison.InvariantCulture))
            {
                MenuUsuario.ImprimirMensagem("\n[Confirmação]: Deseja  deletar a conta?\ntecle (S) para sim e (N) não: ");
                string teclaConfirm = MenuUsuario.ReceberValorInserido();
                string opcaoUsuario = string.IsNullOrWhiteSpace(teclaConfirm) ? null : teclaConfirm.ToUpper();


                if (opcaoUsuario == null || (opcaoUsuario != "S" && opcaoUsuario != "N"))
                {
                    MenuUsuario.ImprimirErro("Informe uma opção valida, tente novamente.");
                    MenuUsuario.Continuar();

                    return;
                }
                else
                {
                    if (opcaoUsuario == "S")
                    {
                        BancoDadosSistema.DeletarDocumentosUsuario(_usuario);
                        BancoDadosSistema.DeletarDocumentosVinculadosAoUsuario(_usuario);
                        BancoDadosSistema.DeletarUsuario(_usuario);

                        MenuUsuario.ImprimirMensagem("\nUsuário deletado com sucesso!");
                        MenuUsuario.Continuar();

                        _usuario = null;
                    }
                    else
                    {
                        MenuUsuario.Continuar();

                        return;
                    }
                }
            }
            else
            {
                MenuUsuario.ImprimirErro("Credenciais inválidas, tente novamente.");
                MenuUsuario.Continuar();
                return;
            }
        }

        public static bool ValidarSenha(string senha)
        {
            bool validaCaracterEspecial = senha.Any(item => !char.IsLetterOrDigit(item));
            bool validaLetraMaiscula = senha.Any(item => char.IsUpper(item));
            bool validaNumero = senha.Any(item => char.IsDigit(item));

            return validaCaracterEspecial && validaLetraMaiscula && validaNumero && senha.Length > 7;
        }
        #endregion

        #region .:Documentos:
        public static string ConstruirListaDocumentos(IEnumerable<Documento> listaDocumentos)
        {
            string retornoListaDocuemntos = $"\n || DOCUMENTOS ||\n";

            foreach (var item in listaDocumentos)
            {
                retornoListaDocuemntos += $"\nId do documento: {item.IdDocumento}\nNome: {item.Nome}\nDescrição: {item.Descricao}\n" +
                $"Conteudo(hash): {item.Conteudo}\nAssinante: {item.Assinador}\nStatus: {item.Status}\n";
            }

            return retornoListaDocuemntos;
        }
        public static string ConstruirListaAssinantes(IEnumerable<Usuario> listaAssinantes)
        {
            string retornoListaDocuementos = $"\n||USUÁRIOS||\n";

            foreach (var item in listaAssinantes)
            {
                retornoListaDocuementos += $"\nNome: {item.Nome}\n";
            }

            return retornoListaDocuementos;
        }

        public static void ListarDocumentosAssinados()
        {
            ValidaLoginUsuario();

            var listaDocumentos = BancoDadosSistema.BuscarDocumentosVinculadosAoUsuario(_usuario, StatusDocumento.Assinado);

            if (listaDocumentos == null || listaDocumentos.Count() == 0)
            {
                MenuUsuario.ImprimirMensagem("\nNenhum documento foi assinado.");
                MenuUsuario.Continuar();

                return;
            }
            else
            {
                string imprimirLista = ConstruirListaDocumentos(listaDocumentos);

                MenuUsuario.ImprimirMensagem(imprimirLista);

                MenuUsuario.Continuar();

                return;
            }

        }
        public static void ListarAtributosDocumentosUsuario()
        {
            ValidaLoginUsuario();

            var listaDocumentos = BancoDadosSistema.BuscarDocumentos(_usuario);

            if (listaDocumentos == null || listaDocumentos.Count() == 0)
            {
                MenuUsuario.ImprimirMensagem("\nNenhum documento foi cadastrado.");
                MenuUsuario.Continuar();

                return;
            }
            else
            {
                string imprimirLista = ConstruirListaDocumentos(listaDocumentos);

                MenuUsuario.ImprimirMensagem(imprimirLista);

                MenuUsuario.Continuar();

                return;
            }
        }

        public static void CadastrarDocumento()
        {
            ValidaLoginUsuario();

            MenuUsuario.ImprimirMensagem("\n|| CADASTRO DE DOCUMENTO||\n\nNome:");

            string nomeDocumento = MenuUsuario.ReceberValorInserido();

            if (string.IsNullOrWhiteSpace(nomeDocumento))
            {
                MenuUsuario.ImprimirErro("O nome do documento deve ser informado, tente novamente.");
                MenuUsuario.Continuar();

                return;
            }
            else
            {
                MenuUsuario.ImprimirMensagem("\nDescrição: ");
                string descricao = MenuUsuario.ReceberValorInserido();

                if (string.IsNullOrWhiteSpace(descricao))
                {
                    MenuUsuario.ImprimirErro("Descrição deve ser informada, tente novamente.");
                    MenuUsuario.Continuar();

                    return;
                }
                else
                {
                    var listaDocumentos = ListarNomesAssinantes();

                    if (listaDocumentos != null)
                    {
                        MenuUsuario.ImprimirMensagem("\nDefina o usuário que terá permissão para verificar a assinatura neste documento: ");
                        string assinador = MenuUsuario.ReceberValorInserido();

                        if (string.IsNullOrWhiteSpace(assinador))
                        {
                            MenuUsuario.ImprimirErro("Informe um assinador, tente novamente.");
                            MenuUsuario.Continuar();

                            return;
                        }
                        else
                        {
                            var usuarioAssinante = listaDocumentos.FirstOrDefault(usr => usr.Nome == assinador);

                            if (usuarioAssinante == null)
                            {
                                MenuUsuario.ImprimirErro("Informe um assinador válido, tente novamente.");
                                MenuUsuario.Continuar();

                                return;
                            }
                            else
                            {
                                int Idocumento = BancoDadosSistema.RetornarQuantidadeDocumento() + 1;

                                string hashDecricao = SegurancaSistema.GerarHash(descricao, Idocumento.ToString());

                                Documento documentoInserir = new Documento(nomeDocumento, descricao, hashDecricao, Idocumento, usuarioAssinante, _usuario, StatusDocumento.Pendente);

                                BancoDadosSistema.InserirDocumento(documentoInserir);

                                MenuUsuario.ImprimirMensagem("\nDocumento cadastrado com sucesso!");
                                MenuUsuario.Continuar();

                                return;
                            }
                        }
                    }
                    else
                    {
                        MenuUsuario.ImprimirErro("Nenhum assinante foi encontrado.");
                        MenuUsuario.Continuar();

                        return;
                    }
                }
            }
        }

        public static void DeletarDocumento()
        {
            ValidaLoginUsuario();

            var listaDocumentos = BancoDadosSistema.BuscarDocumentos(_usuario, StatusDocumento.Pendente);

            if (listaDocumentos == null || listaDocumentos.Count() == 0)
            {
                MenuUsuario.ImprimirMensagem("\nNenhum Documento pendente, para ser deletado!");
                MenuUsuario.Continuar();

                return;
            }
            else
            {
                string imprimirLista = ConstruirListaDocumentos(listaDocumentos);

                MenuUsuario.ImprimirMensagem($"{imprimirLista}\nInforme o ID do documento que deseja ser deletado: ");
                string idDocumento = MenuUsuario.ReceberValorInserido();

                int nroId;

                if (Int32.TryParse(idDocumento, out nroId))
                {
                    var documento = listaDocumentos.FirstOrDefault(usr => usr.IdUsuarioDocumento == _usuario.Id && usr.IdDocumento == nroId && usr.Status == StatusDocumento.Pendente);

                    if (documento != null)
                    {
                        documento.Status = StatusDocumento.Removido;

                        BancoDadosSistema.AtualizarDocumento(documento);

                        MenuUsuario.ImprimirMensagem("\nDocumento deletado com sucesso!");
                        MenuUsuario.Continuar();

                        return;
                    }
                    else
                    {
                        MenuUsuario.ImprimirErro("Documento não existe, tente novamente.");
                        MenuUsuario.Continuar();

                        return;
                    }
                }
                else
                {
                    MenuUsuario.ImprimirErro("O ID do documento deve ser informado, tente novamente.");
                    MenuUsuario.Continuar();

                    return;
                }
            }
        }

        public static void AssinarDocumento()
        {
            ValidaLoginUsuario();

            var listaDocumentos = BancoDadosSistema.BuscarDocumentosVinculadosAoUsuario(_usuario, StatusDocumento.Pendente);

            if (listaDocumentos == null || listaDocumentos.Count() == 0)
            {
                MenuUsuario.ImprimirMensagem("\nNenhum documento a ser assinado no momento.");
                MenuUsuario.Continuar();

                return;
            }
            else
            {
                string imprimirLista = ConstruirListaDocumentos(listaDocumentos);

                MenuUsuario.ImprimirMensagem($"{imprimirLista}\nInforme o ID do documento a ser assinado: ");
                string idDocumento = MenuUsuario.ReceberValorInserido();

                int nroId;

                if (Int32.TryParse(idDocumento, out nroId))
                {
                    var documento = listaDocumentos.FirstOrDefault(usr => usr.Assinador == _usuario.Nome && usr.IdDocumento == nroId);

                    if (documento != null)
                    {
                        byte[] assinatura = SegurancaSistema.GerarAssinatura(documento, _usuario.ChavePrivada);

                        if (assinatura != null)
                        {
                            MenuUsuario.ImprimirMensagem("Documento assinado com sucesso!");
                            MenuUsuario.Continuar();

                            string hashAssinatura = Convert.ToBase64String(assinatura);

                            documento.Assinatura = hashAssinatura;
                            documento.Status = StatusDocumento.Assinado;
                            documento.ChavePublica = _usuario.ChavePublica;

                            BancoDadosSistema.AtualizarDocumento(documento);

                            return;
                        }
                        else
                        {
                            MenuUsuario.ImprimirErro("A geração da assinatura não obteve sucesso.");
                            MenuUsuario.Continuar();

                            return;
                        }
                    }
                    else
                    {
                        MenuUsuario.ImprimirErro("Informe uma opção válida, tente novamente.");
                        MenuUsuario.Continuar();

                        return;
                    }
                }
                else
                {

                    MenuUsuario.ImprimirErro("O ID do documento deve ser informado, tente novamente.");
                    MenuUsuario.Continuar();

                    return;
                }
            }
        }
        public static void VerificarAssinatura()
        {
            ValidaLoginUsuario();

            var listaDocumentos = BancoDadosSistema.BuscarDocumentos(_usuario, StatusDocumento.Assinado);

            if (listaDocumentos == null || listaDocumentos.Count() == 0)
            {
                MenuUsuario.ImprimirMensagem("\nNenhum documento a ser verificado no momento.");
                MenuUsuario.Continuar();

                return;
            }
            else
            {
                string imprimirLista = ConstruirListaDocumentos(listaDocumentos);

                MenuUsuario.ImprimirMensagem($"{imprimirLista}\nInforme o ID do documento a ser verificado: ");
                string idDocumento = MenuUsuario.ReceberValorInserido();

                int nroId;

                if (Int32.TryParse(idDocumento, out nroId))
                {
                    Documento documento = listaDocumentos.FirstOrDefault(usr => usr.IdUsuarioDocumento == _usuario.Id && usr.IdDocumento == nroId && usr.Status == StatusDocumento.Assinado);

                    if (documento != null)
                    {
                        bool validaAssinatura = SegurancaSistema.VerificarAssinatura(documento);

                        if (validaAssinatura)
                        {
                            MenuUsuario.ImprimirMensagem("Assinatura válida!");
                            MenuUsuario.Continuar();

                            return;
                        }
                        else
                        {
                            MenuUsuario.ImprimirMensagem("Assinatura inválida.");
                            MenuUsuario.Continuar();

                            return;
                        }
                    }
                    else
                    {
                        MenuUsuario.ImprimirErro("Documento não encontrado, tente novamente.");
                        MenuUsuario.Continuar();

                        return;
                    }
                }
                else
                {
                    MenuUsuario.ImprimirErro("O ID do documento deve ser informado, tente novamente.");
                    MenuUsuario.Continuar();

                    return;
                }
            }
        }
        #endregion
    }
}