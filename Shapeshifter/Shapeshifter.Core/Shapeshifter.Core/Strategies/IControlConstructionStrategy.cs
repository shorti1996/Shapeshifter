﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shapeshifter.Core.Strategies
{
    public interface IControlConstructionStrategy<TControlType, TDataType>
    {
        TControlType ConstructControl(TDataType data);
    }
}