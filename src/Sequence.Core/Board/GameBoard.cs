using Sequence.Core.Cards;

namespace Sequence.Core.Board;

public sealed class GameBoard
{
    private readonly Cell[,] _cells;
    private readonly ILookup<Card, Cell> _cellsByCard;

    public GameBoard()
    {
        var layout = BoardLayout.Generate();
        _cells = new Cell[BoardLayout.Size, BoardLayout.Size];

        var allCells = new List<Cell>(BoardLayout.Size * BoardLayout.Size);
        for (var row = 0; row < BoardLayout.Size; row++)
        {
            for (var col = 0; col < BoardLayout.Size; col++)
            {
                var cell = new Cell { Row = row, Col = col, Card = layout[row, col] };
                _cells[row, col] = cell;
                allCells.Add(cell);
            }
        }

        _cellsByCard = allCells.Where(c => c.Card is not null).ToLookup(c => c.Card!.Value);
    }

    public int Size => BoardLayout.Size;

    public Cell GetCell(int row, int col) => _cells[row, col];

    public IEnumerable<Cell> AllCells()
    {
        for (var row = 0; row < BoardLayout.Size; row++)
            for (var col = 0; col < BoardLayout.Size; col++)
                yield return _cells[row, col];
    }

    /// <summary>The two board cells matching a given non-jack card.</summary>
    public IEnumerable<Cell> CellsForCard(Card card) => _cellsByCard[card];

    public IEnumerable<Cell> OpenCellsForCard(Card card) =>
        CellsForCard(card).Where(c => c.ChipOwnerId is null);

    public IEnumerable<Cell> AllOpenNonCornerCells() =>
        AllCells().Where(c => !c.IsFree && c.ChipOwnerId is null);
}
