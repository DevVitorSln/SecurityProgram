using LiteDB;
using SecurityProgram.Enums;
using SecurityProgram.Models;

namespace SecurityProgram.Funcoes
{
    public class BancoDadosSistema
    {
        private readonly static LiteDatabase _conexaoDb;

        static BancoDadosSistema()
        {
            Configuracao config = SegurancaSistema.Config;

            if (config == null)
            {
                throw new ArgumentNullException();
            }
            else
            {
                _conexaoDb = new LiteDatabase($"Filename={config.CaminhoBanco};Password={config.SenhaBanco};Collation={new Collation("pt-BR/None")}");
            }
        }

        public static void Dispose()
        {
            _conexaoDb.Dispose();
        }

        #region .:Usuarios:
        public static IEnumerable<Usuario> CarregarAssinantes(Usuario usuario)
        {
            IEnumerable<Usuario> listaUsuarios = _conexaoDb.GetCollection<Usuario>().Find(usr => usr.Nome != usuario.Nome);

            return listaUsuarios;
        }

        public static bool VerificarUsuario(string nome)
        {
            return _conexaoDb.GetCollection<Usuario>().Exists(usr => usr.Nome.Equals(nome, StringComparison.InvariantCulture));
        }

        public static Usuario RetornarUsuario(string nome, string senha)
        {
            Usuario usuario = _conexaoDb.GetCollection<Usuario>().FindOne(usr => usr.Nome.Equals(nome, StringComparison.InvariantCulture) && usr.Senha.Equals(senha, StringComparison.InvariantCulture));

            return usuario;
        }

        public static void InserirUsuario(Usuario usuario)
        {
            _conexaoDb.GetCollection<Usuario>().Insert(usuario);
        }

        public static void AtualizarUsuario(Usuario usuario)
        {
            _conexaoDb.GetCollection<Usuario>().Update(usuario.Id, usuario);
        }

        public static void DeletarUsuario(Usuario usuario)
        {
            _conexaoDb.GetCollection<Usuario>().Delete(usuario.Id);
        }
        #endregion

        #region .:Documentos:
        public static IEnumerable<Documento> BuscarDocumentos(Usuario usuario)
        {
            var listaDocumento = _conexaoDb.GetCollection<Documento>().Find(usr => usr.IdUsuarioDocumento == usuario.Id && usr.Status != StatusDocumento.Removido);

            return listaDocumento;
        }
        public static IEnumerable<Documento> BuscarDocumentos(Usuario usuario, StatusDocumento statusDocumento)
        {
            IEnumerable<Documento> listaDocumentos;

            if (statusDocumento == StatusDocumento.Assinado)
            {
                listaDocumentos = _conexaoDb.GetCollection<Documento>().Find(usr => usr.IdUsuarioDocumento == usuario.Id && usr.Status == StatusDocumento.Assinado);
            }
            else if (statusDocumento == StatusDocumento.Pendente)
            {
                listaDocumentos = _conexaoDb.GetCollection<Documento>().Find(usr => usr.IdUsuarioDocumento == usuario.Id && usr.Status == StatusDocumento.Pendente);
            }
            else
            {
                listaDocumentos = _conexaoDb.GetCollection<Documento>().Find(usr => usr.IdUsuarioDocumento == usuario.Id && usr.Status == StatusDocumento.Removido);
            }

            return listaDocumentos;
        }
        public static IEnumerable<Documento> BuscarDocumentosVinculadosAoUsuario(Usuario usuario, StatusDocumento statusDocumento)
        {
            IEnumerable<Documento> listaDocumentos;

            if (statusDocumento == StatusDocumento.Pendente)
            {
                listaDocumentos = _conexaoDb.GetCollection<Documento>().Find(usr => usr.Assinador == usuario.Nome && usr.Status == StatusDocumento.Pendente);
            }
            else if (statusDocumento == StatusDocumento.Assinado)
            {
                listaDocumentos = _conexaoDb.GetCollection<Documento>().Find(usr => usr.Assinador == usuario.Nome && usr.Status == StatusDocumento.Assinado);
            }
            else
            {
                listaDocumentos = _conexaoDb.GetCollection<Documento>().Find(usr => usr.Assinador == usuario.Nome && usr.Status == StatusDocumento.Removido);
            }

            return listaDocumentos;
        }

        public static int RetornarQuantidadeDocumento()
        {
            int documento = _conexaoDb.GetCollection<Documento>().Count();

            return documento;
        }

        public static void InserirDocumento(Documento documento)
        {
            _conexaoDb.GetCollection<Documento>().Insert(documento);
        }

        public static void AtualizarDocumento(Documento documento)
        {
            _conexaoDb.GetCollection<Documento>().Update(documento._id, documento);
        }

        public static void DeletarDocumentosUsuario(Usuario usuario)
        {
            var listadocumento = _conexaoDb.GetCollection<Documento>().Find(usr => usr.IdUsuarioDocumento == usuario.Id && usr.Status == StatusDocumento.Pendente);

            foreach (var item in listadocumento)
            {
                item.Status = StatusDocumento.Removido;
                _conexaoDb.GetCollection<Documento>().Update(listadocumento);
            }
        }
        public static void DeletarDocumentosVinculadosAoUsuario(Usuario usuario)
        {
            IEnumerable<Documento> listaDocumentos = _conexaoDb.GetCollection<Documento>().Find(usr => usr.Assinador == usuario.Nome && usr.Status == StatusDocumento.Pendente);

            foreach (var item in listaDocumentos)
            {
                item.Status = StatusDocumento.Removido;
                _conexaoDb.GetCollection<Documento>().Update(item._id, item);
            }
        }
        #endregion
    }
}