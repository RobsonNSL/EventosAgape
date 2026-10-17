namespace AgapeApp.Application.Common;

/// <summary>
/// Regras da foto de perfil do usuário: formatos aceitos, tamanho máximo e
/// conferência da assinatura do arquivo (garante que é mesmo uma imagem).
/// </summary>
public static class FotoPerfil
{
    /// <summary>Tamanho máximo aceito para a imagem (5 MB).</summary>
    public const long TamanhoMaximoBytes = 5 * 1024 * 1024;

    /// <summary>Quantidade de bytes lidos para conferir a assinatura do arquivo.</summary>
    public const int BytesAssinatura = 12;

    private static readonly string[] ExtensoesPermitidas = [".jpg", ".jpeg", ".png", ".webp"];

    /// <summary>Diz se o nome do arquivo tem uma extensão de imagem aceita.</summary>
    public static bool ExtensaoPermitida(string? nomeArquivo)
        => !string.IsNullOrWhiteSpace(nomeArquivo)
           && ExtensoesPermitidas.Contains(Path.GetExtension(nomeArquivo).ToLowerInvariant());

    /// <summary>Extensão normalizada (sem ponto, minúscula) usada no nome do arquivo salvo.</summary>
    public static string ExtensaoNormalizada(string nomeArquivo)
    {
        var extensao = Path.GetExtension(nomeArquivo).TrimStart('.').ToLowerInvariant();
        return extensao == "jpeg" ? "jpg" : extensao;
    }

    /// <summary>Confere a assinatura (magic bytes) de JPEG, PNG ou WEBP.</summary>
    public static bool ConteudoEhImagem(ReadOnlySpan<byte> inicio)
    {
        // JPEG: FF D8 FF
        if (inicio.Length >= 3 && inicio[0] == 0xFF && inicio[1] == 0xD8 && inicio[2] == 0xFF)
            return true;

        // PNG: 89 50 4E 47 0D 0A 1A 0A
        if (inicio.Length >= 8
            && inicio[0] == 0x89 && inicio[1] == 0x50 && inicio[2] == 0x4E && inicio[3] == 0x47
            && inicio[4] == 0x0D && inicio[5] == 0x0A && inicio[6] == 0x1A && inicio[7] == 0x0A)
        {
            return true;
        }

        // WEBP: "RIFF" .... "WEBP"
        if (inicio.Length >= 12
            && inicio[0] == 0x52 && inicio[1] == 0x49 && inicio[2] == 0x46 && inicio[3] == 0x46
            && inicio[8] == 0x57 && inicio[9] == 0x45 && inicio[10] == 0x42 && inicio[11] == 0x50)
        {
            return true;
        }

        return false;
    }
}
