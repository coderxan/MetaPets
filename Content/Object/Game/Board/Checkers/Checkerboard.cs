using System;
using System.Collections;

using Server;

namespace Server.Items
{
    public class CheckerBoard : BaseBoard
    {
        public override int LabelNumber { get { return 1016449; } } // a checker board

        [Constructable]
        public CheckerBoard()
            : base(0xFA6)
        {
        }

        public override void CreatePieces()
        {
            for (int i = 0; i < 4; i++)
            {
                CreatePiece(new PieceWhiteChecker(this), (50 * i) + 45, 25);
                CreatePiece(new PieceWhiteChecker(this), (50 * i) + 70, 50);
                CreatePiece(new PieceWhiteChecker(this), (50 * i) + 45, 75);
                CreatePiece(new PieceBlackChecker(this), (50 * i) + 70, 150);
                CreatePiece(new PieceBlackChecker(this), (50 * i) + 45, 175);
                CreatePiece(new PieceBlackChecker(this), (50 * i) + 70, 200);
            }
        }

        public CheckerBoard(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }

    #region Checkerboard Pieces

    public class PieceWhiteChecker : BasePiece
    {
        public override string DefaultName
        {
            get { return "white checker"; }
        }

        public PieceWhiteChecker(BaseBoard board)
            : base(0x3584, board)
        {
        }

        public PieceWhiteChecker(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }

    public class PieceBlackChecker : BasePiece
    {
        public override string DefaultName
        {
            get { return "black checker"; }
        }

        public PieceBlackChecker(BaseBoard board)
            : base(0x358B, board)
        {
        }

        public PieceBlackChecker(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }

    #endregion
}