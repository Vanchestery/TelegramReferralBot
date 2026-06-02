using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ReferralBot.Db.Helpers;

/// <summary>
/// Кастомный ValueComparer для List&lt;string&gt;, хранящегося в jsonb-колонке.
///
/// Зачем нужен:
/// EF Core по умолчанию сравнивает ссылки на объекты (reference equality).
/// Для jsonb-колонки с List&lt;string&gt; это приводит к тому, что EF Core всегда
/// считает значение "изменённым" и генерирует лишние UPDATE-запросы.
/// Этот компаратор учит EF Core сравнивать содержимое списков, а не ссылки.
/// </summary>
public class ListOfStringsComparer() : ValueComparer<List<string>>(
    (a, b) => (a == null && b == null) || (a != null && b != null && a.SequenceEqual(b)),
    list => list == null ? 0 : list.Aggregate(0, (acc, s) => HashCode.Combine(acc, s.GetHashCode())),
    list => list == null ? new List<string>() : list.ToList());
