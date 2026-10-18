using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace AgapeApp.Mobile.Helpers;

/// <summary>
/// ObservableCollection que troca todo o conteúdo com uma única notificação.
/// O Clear + Add item a item disparava um redesenho da lista para cada linha,
/// o que deixava as telas engasgadas no celular.
/// </summary>
public class ColecaoObservavel<T> : ObservableCollection<T>
{
    public void Substituir(IEnumerable<T> itens)
    {
        Items.Clear();

        foreach (var item in itens)
            Items.Add(item);

        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
        OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }
}
