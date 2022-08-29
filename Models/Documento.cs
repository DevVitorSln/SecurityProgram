using LiteDB;
using SecurityProgram.Enums;

namespace SecurityProgram.Models
{
    public class Documento
    {
        public ObjectId _id { get; private set; }
        public int IdDocumento { get; private set; }
        public string Nome { get; private set; }
        public string Descricao { get; private set; }
        public string Conteudo { get; private set; }
        public Guid IdUsuarioDocumento { get; private set; }
        public string Assinatura { get; set; }
        public string Assinador { get; private set; }
        public string ChavePublica { get; set; }
        public StatusDocumento Status { get; set; }

        public Documento() { }

        public Documento(string nome, string descricao, string conteudo, int idDocumento, Usuario assinador, Usuario usuario, StatusDocumento status)
        {
            Nome = nome;
            Descricao = descricao;
            Conteudo = conteudo;
            IdUsuarioDocumento = usuario.Id;
            IdDocumento = idDocumento;
            Assinador = assinador.Nome;
            Status = status;
        }

    }
}