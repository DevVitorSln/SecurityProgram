namespace SecurityProgram.Models
{
    public class Usuario
    {
        public string Nome { get; private set; }
        public string Senha { get; set; }
        public string ChavePrivada { get; private set; }
        public string ChavePublica { get; private set; }
        public Guid Id { get; private set; }

        public Usuario(string nome, string senha, string chavePublica, string chavePrivada, Guid id)
        {
            Nome = nome;
            Senha = senha;
            ChavePublica = chavePublica;
            ChavePrivada = chavePrivada;
            Id = id;
        }
    }
}