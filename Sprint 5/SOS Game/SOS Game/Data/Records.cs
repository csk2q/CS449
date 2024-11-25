namespace SOS_Game;

public record Position(int row, int column);

public record Sos(Position S1, Position O, Position S2);

public record Move(TileType Tile, Position Position);

public record TurnResult(Move Move, Sos[] SosMade, PlayerType placingPlayer);