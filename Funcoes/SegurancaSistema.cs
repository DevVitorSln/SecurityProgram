using Newtonsoft.Json;
using SecurityProgram.Models;
using System.Net;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace SecurityProgram.Funcoes
{
    public class SegurancaSistema
    {
        internal static Configuracao Config = null;

        private static string _salt = null;

        private static string _arquivo = @"config.json";

        public static SecureString MascararSenha()
        {
            var senha = new SecureString();

            ConsoleKeyInfo KeyInfo;

            do
            {
                KeyInfo = Console.ReadKey(true);

                if (!char.IsControl(KeyInfo.KeyChar))
                {
                    senha.AppendChar(KeyInfo.KeyChar);
                    MenuUsuario.ImprimirMensagem("*");
                }
                else if (KeyInfo.Key == ConsoleKey.Backspace && senha.Length > 0)
                {
                    senha.RemoveAt(senha.Length - 1);
                    MenuUsuario.ImprimirMensagem("\b \b");
                }
            }
            while (KeyInfo.Key != ConsoleKey.Enter);

            return senha;
        }

        public static RSA GeraParDeChaves()
        {
            RSA rsaKey = RSA.Create(2048);

            return rsaKey;
        }

        public static void CarregarConfiguracao()
        {
            try
            {
                string conteudoArquivo = File.ReadAllText(_arquivo);

                if (conteudoArquivo == null)
                {
                    throw new ArgumentNullException("\nArquivo não encontrado.");
                }

                Config = JsonConvert.DeserializeObject<Configuracao>(conteudoArquivo);

                VerificaSalt();

                Console.Clear();
                MenuUsuario.ImprimirMensagem("Senha para Criptografar o arquivo: ");
                var password = MascararSenha();
                string rawPassword = new NetworkCredential(string.Empty, password).Password;

                byte[] byteArrayArquivo = Encoding.UTF8.GetBytes(conteudoArquivo);

                byte[] arquivoCriptografado = Criptografar(byteArrayArquivo, rawPassword);

                File.WriteAllBytes(_arquivo, arquivoCriptografado);
            }
            catch (JsonException)
            {
                byte[] conteudoArquivo = File.ReadAllBytes(_arquivo);

                Console.Clear();
                MenuUsuario.ImprimirMensagem("Senha para Descriptografar o arquivo: ");
                var senha = MascararSenha();
                string rawPassword = new NetworkCredential(string.Empty, senha).Password;

                byte[] arquivoDescriptografado = Descriptografar(conteudoArquivo, rawPassword);

                string arquivoString = Encoding.UTF8.GetString(arquivoDescriptografado);

                Config = JsonConvert.DeserializeObject<Configuracao>(arquivoString);

                VerificaSalt();
            }
            catch (Exception)
            {
                throw new ArgumentNullException();
            }
        }

        public static byte[] Criptografar(byte[] arquivo, string senha)
        {
            var hashProvider = HashAlgorithm.Create("SHA256");

            byte[] key = hashProvider.ComputeHash(Encoding.UTF8.GetBytes(senha));

            using var aes = Aes.Create();

            aes.Key = key;
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;

            try
            {
                using ICryptoTransform encryptor = aes.CreateEncryptor();

                byte[] results = encryptor.TransformFinalBlock(arquivo, 0, arquivo.Length);

                return results;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static byte[] Descriptografar(byte[] arquivo, string senha)
        {
            var hashProvider = HashAlgorithm.Create("SHA256");

            byte[] key = hashProvider.ComputeHash(Encoding.UTF8.GetBytes(senha));

            using var aes = Aes.Create();

            aes.Key = key;
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;

            try
            {
                using ICryptoTransform decryptor = aes.CreateDecryptor();

                byte[] results = decryptor.TransformFinalBlock(arquivo, 0, arquivo.Length);

                return results;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string GerarHash(string conteudo, string idDocumento = null, string nomeAlgoritmoHash = null)
        {
            if (nomeAlgoritmoHash == null && idDocumento == null)
            {
                var hashAlgoritmo = HashAlgorithm.Create("SHA256");

                if (hashAlgoritmo == null)
                    throw new ArgumentNullException("\nO algoritmo de HASH é inválido.");

                var encodedValue = Encoding.UTF8.GetBytes(conteudo + _salt);
                var encryptedPassword = hashAlgoritmo.ComputeHash(encodedValue);
                string hashToBase64 = Convert.ToBase64String(encryptedPassword);

                return hashToBase64;
            }
            else if (nomeAlgoritmoHash != null && idDocumento == null)
            {
                var hashAlgoritmo = HashAlgorithm.Create(nomeAlgoritmoHash);

                if (hashAlgoritmo == null)
                    throw new ArgumentNullException("\nO algoritmo de HASH é inválido.");

                var encodedValue = Encoding.UTF8.GetBytes(conteudo);
                var encryptedPassword = hashAlgoritmo.ComputeHash(encodedValue);
                string hashToBase64 = Convert.ToBase64String(encryptedPassword);

                return hashToBase64;
            }
            else
            {
                var hashAlgoritmo = HashAlgorithm.Create("SHA256");

                if (hashAlgoritmo == null)
                    throw new ArgumentNullException("\nO algoritmo de HASH é inválido.");

                int intIdDocumento;

                var opcaoIntValida = Int32.TryParse(idDocumento, out intIdDocumento);

                if (opcaoIntValida)
                {
                    var encodedValue = Encoding.UTF8.GetBytes(conteudo + intIdDocumento);
                    var encryptedPassword = hashAlgoritmo.ComputeHash(encodedValue);
                    string hashToBase64 = Convert.ToBase64String(encryptedPassword);

                    return hashToBase64;
                }
                else
                {
                    throw new ArgumentException("\nNão foi possível converter o idDocumento para Int.");
                }
            }

        }

        public static bool VerificarAssinatura(Documento documento)
        {
            string dadoOriginal = documento.Descricao + documento.IdDocumento;
            byte[] byteDadoOriginal = Encoding.UTF8.GetBytes(dadoOriginal);

            string assinatura = documento.Assinatura;
            byte[] byteAssinatura = Convert.FromBase64String(assinatura);

            string publicKey = documento.ChavePublica;

            byte[] bytePublicKey = Convert.FromBase64String(publicKey);

            RSA rsaKey = RSA.Create();

            int bytesRead = 0;

            rsaKey.ImportRSAPublicKey(bytePublicKey, out bytesRead);

            return rsaKey.VerifyData(byteDadoOriginal, byteAssinatura, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
        public static byte[] GerarAssinatura(Documento documento, string privateKey)
        {
            string hashDocumento = documento.Conteudo;

            byte[] byteHashDocumento = Convert.FromBase64String(hashDocumento);

            byte[] bytePrivateKey = Convert.FromBase64String(privateKey);

            RSA rsaKey = RSA.Create();

            int bytesRead = 0;

            rsaKey.ImportRSAPrivateKey(bytePrivateKey, out bytesRead);

            byte[] byteAssinatura = rsaKey.SignHash(byteHashDocumento, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            return byteAssinatura;
        }

        public static void VerificaSalt()
        {
            if (Config.Salt == null)
            {
                Config.Salt = GerarSalt();

                var configSerialize = JsonConvert.SerializeObject(Config, Formatting.Indented);

                File.WriteAllText(_arquivo, configSerialize);

                _salt = Config.Salt;
            }
            else
            {
                _salt = Config.Salt;
            }
        }
        public static string GerarSalt()
        {
            var rng = RandomNumberGenerator.Create();
            var buff = new byte[10];
            rng.GetBytes(buff);

            return _salt = Convert.ToBase64String(buff);

        }

    }
}