namespace NeatShot.Core.Services;

/// <summary>
/// Quản lý ngăn xếp hoàn tác (Undo Stack) theo nguyên tắc LIFO (Last In First Out),
/// hỗ trợ duyệt danh sách phần tử và thông báo sự kiện StateChanged.
/// </summary>
public class UndoStack<T>
{
    private readonly List<T> _items = new();

    /// <summary>
    /// Kích hoạt khi có thay đổi trong ngăn xếp (Push, Pop, Clear).
    /// </summary>
    public event EventHandler? StateChanged;

    /// <summary>
    /// Số lượng phần tử hiện có trong ngăn xếp.
    /// </summary>
    public int Count => _items.Count;

    /// <summary>
    /// Có thể hoàn tác (Undo) hay không (khi số phần tử lớn hơn 0).
    /// </summary>
    public bool CanUndo => _items.Count > 0;

    /// <summary>
    /// Danh sách các phần tử hiện thời từ cũ nhất đến mới nhất.
    /// </summary>
    public IReadOnlyList<T> Items => _items.AsReadOnly();

    /// <summary>
    /// Thêm một phần tử mới vào đỉnh ngăn xếp.
    /// </summary>
    public void Push(T item)
    {
        _items.Add(item);
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Lấy và xóa phần tử mới nhất khỏi đỉnh ngăn xếp. Trả về default nếu rỗng.
    /// </summary>
    public T? Pop()
    {
        if (_items.Count == 0)
        {
            return default;
        }

        var lastIndex = _items.Count - 1;
        var item = _items[lastIndex];
        _items.RemoveAt(lastIndex);

        StateChanged?.Invoke(this, EventArgs.Empty);
        return item;
    }

    /// <summary>
    /// Xóa toàn bộ phần tử trong ngăn xếp.
    /// </summary>
    public void Clear()
    {
        if (_items.Count == 0)
        {
            return;
        }

        _items.Clear();
        StateChanged?.Invoke(this, EventArgs.Empty);
    }
}
