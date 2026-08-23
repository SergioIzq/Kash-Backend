namespace Kash.Shared.Application.Interfaces;

/// <summary>
/// Genera y hashea tokens de API personales (kash_pat_...). El valor en claro solo existe
/// en el momento de la generación; a partir de ahí solo se persiste y compara el hash.
/// </summary>
public interface IApiTokenHasher
{
    /// <summary>Genera un nuevo token de alta entropía junto con su hash para persistir.</summary>
    (string PlainToken, string Hash) GenerateToken();

    /// <summary>Calcula el hash de un token en claro, para comparar contra el valor persistido.</summary>
    string Hash(string token);
}
