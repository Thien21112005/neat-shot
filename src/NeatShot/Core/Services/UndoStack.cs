namespace NeatShot.Core.Services;

/// <summary>
/// Quản lý ngăn xếp hoàn tác (Undo Stack) theo nguyên tắc LIFO (Last In First Out),
/// hỗ trợ duyệt danh sách phần tử và thông báo sự kiện StateChanged.
/// </summary>
public class UndoStack<T>
{
    private readonly List<T> _items = new();
    private readonly List<T> _redoItems = new();

    /// <summary>
    /// Kích hoạt khi có thay đổi trong ngăn xếp (Push, Pop, Redo, Clear).
    /// </summary>
    public event EventHandler? StateChanged;

    /// <summary>
    /// Số lượng phần tử hiện có trong ngăn xếp hoàn tác.
    /// </summary>
    public int Count => _items.Count;

    /// <summary>
    /// Có thể hoàn tác (Undo) hay không (khi số phần tử lớn hơn 0).
    /// </summary>
    public bool CanUndo => _items.Count > 0;

    /// <summary>
    /// Có thể làm lại (Redo) hay không (khi có phần tử trong ngăn xếp làm lại).
    /// </summary>
    public bool CanRedo => _redoItems.Count > 0;

    /// <summary>
    /// Danh sách các phần tử hiện thời từ cũ nhất đến mới nhất.
    /// </summary>
    public IReadOnlyList<T> Items => _items.AsReadOnly();

    /// <summary>
    /// Thêm một phần tử mới vào đỉnh ngăn xếp (và xóa lịch sử Redo).
    /// </summary>
    public void Push(T item)
    {
        _items.Add(item);
        _redoItems.Clear();
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Lấy và xóa phần tử mới nhất khỏi đỉnh ngăn xếp để chuyển sang Redo. Trả về default nếu rỗng.
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
        _redoItems.Add(item);

        StateChanged?.Invoke(this, EventArgs.Empty);
        return item;
    }

    /// <summary>
    /// Làm lại thao tác vừa hoàn tác. Đưa phần tử từ Redo quay lại Undo.
    /// </summary>
    public T? Redo()
    {
        if (_redoItems.Count == 0)
        {
            return default;
        }

        var lastIndex = _redoItems.Count - 1;
        var item = _redoItems[lastIndex];
        _redoItems.RemoveAt(lastIndex);
        _items.Add(item);

        StateChanged?.Invoke(this, EventArgs.Empty);
        return item;
    }

    /// <summary>
    /// Xóa toàn bộ phần tử trong cả 2 ngăn xếp Undo và Redo.
    /// </summary>
    public void Clear()
    {
        if (_items.Count == 0 && _redoItems.Count == 0)
        {
            return;
        }

        _items.Clear();
        _redoItems.Clear();
        StateChanged?.Invoke(this, EventArgs.Empty);
    }
}
