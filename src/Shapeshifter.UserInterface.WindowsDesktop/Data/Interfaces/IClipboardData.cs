﻿namespace Shapeshifter.Core.Data
{
    public interface IClipboardData
    {
        IDataSource Source { get; }

        byte[] RawData { get; }

        uint RawFormat { get; }
    }
}