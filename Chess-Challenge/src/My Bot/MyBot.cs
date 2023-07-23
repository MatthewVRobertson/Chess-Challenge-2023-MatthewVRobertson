using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Linq;

public class MyBot : IChessBot
{
    int currentTurn = 0;
    const int maxOpeningTurns = 10;
    private Random r = new Random();
    public int lastMoveIndex = 1;

    /*
     * 1-Control the center of the board
     * 2-Develop pieces quickly, knights and bishops
     * 3-Knights before bishops
     * 4-Don't move the same piece twice in the opening
     * 5-Don't bring out the queen too early
     * 6-Castle BEFORE move 10
     * 7-Connect your rooks
     * 8-Move rook to open / half open
     * 9-Knights more beneficial towards center
     * 10-Avoid doubled pawns!!!
     --######################################################
     * Avoid isolated and backwards pawn *pawns with no defence
     * Don't trade a bishop for a knight
     * ???
     * Avoid moving pawns in front of castled king
     */

    public Move Think(Board board, Timer timer)
    {
        Move moveToMake;
        List<Move> moves = board.GetLegalMoves().ToList();
        List<WeightedMoved> weights = new List<WeightedMoved>();

        // initializeBoard
        foreach (Move move in moves)
        {
            WeightedMoved toWeigh = new WeightedMoved(move);
            Weigh(toWeigh, this, board);
            weights.Add(toWeigh);
        }

        var weightGroups = weights.GroupBy(x => x.Score);
        WeightedMoved[] heighestWeight = weightGroups.OrderByDescending(x=>x.Key).First().ToArray();

        moveToMake = heighestWeight[r.Next(0, heighestWeight.Length)].TheMove;
        this.lastMoveIndex = moveToMake.TargetSquare.Index;

        currentTurn++;
        return moveToMake;
    }

    public void Weigh(WeightedMoved weightedMove, MyBot bot, Board board)
    {
        var move = weightedMove.TheMove;

        // TODO - check if only king  & if in check      
        if (move.IsPromotion)
        {
            if (move.PromotionPieceType != PieceType.Queen)
            {
                return;
            }

            if (!board.SquareIsAttackedByOpponent(move.StartSquare))
            {
                weightedMove.Score += 100;
            }
        }

        if (move.TargetSquare.Index != bot.lastMoveIndex)
        {
            weightedMove.Score += 100;
        }

        if (!board.SquareIsAttackedByOpponent(move.TargetSquare))
        {
            weightedMove.Score += 10;
        }

        if (board.SquareIsAttackedByOpponent(move.StartSquare))
        {
            weightedMove.Score += 10 * (int)move.MovePieceType;
        }

        // towards center?
        if (move.IsEnPassant || move.IsCapture)
        {
            weightedMove.Score += 100;

            if (move.CapturePieceType == PieceType.Queen)
            {
                weightedMove.Score += 9;
            }
            else
            {
                weightedMove.Score += (int)move.CapturePieceType;
            }
        }

        // if only king left
        if (bot.currentTurn < maxOpeningTurns)
        {
            if (move.TargetSquare.Rank > move.StartSquare.Rank)
            {
                weightedMove.Score += 10;
            }

            if (move.MovePieceType == PieceType.Pawn)
            {
                weightedMove.Score += 10;
            }

            if (move.IsCastles)
            {
                weightedMove.Score += 100;
            }
        }
    }

    public class WeightedMoved
    {
        public int Score { get; set; }

        public Move TheMove { get; set; }

        public WeightedMoved(Move move)
        {
            TheMove = move;
        }
    }
}