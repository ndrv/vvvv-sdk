﻿#region usings
using System;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
#endregion usings

namespace VVVV.Nodes.Generic
{
	public class GetSpreadAdvanced<T> : IPluginEvaluate
	{
	    #region fields & pins
	    [Input("Input")]
        protected IInStream<IInStream<T>> FInput;
	    
	    [Input("Offset")]
        protected IInStream<int> FOffset;
	    
	    [Input("Count", DefaultValue = 1)]
        protected IInStream<int> FCount;
	
	    [Output("Output")]
        protected IOutStream<IInStream<T>> FOutput;
	    #endregion fields & pins
	
	    //called when data for any output pin is requested
	    public void Evaluate(int spreadMax)
	    {
	        FOutput.Length = StreamUtils.GetMaxLength(FInput, FOffset, FCount);
	
	        var inputBuffer = MemoryPool<IInStream<T>>.GetArray();
	        var offsetBuffer = MemoryPool<int>.GetArray();
	        var countBuffer = MemoryPool<int>.GetArray();
	        
	        try
	        {
	            using (var inputReader = FInput.GetCyclicReader())
	            using (var offsetReader = FOffset.GetCyclicReader())
	            using (var countReader = FCount.GetCyclicReader())
	            using (var outputWriter = FOutput.GetWriter())
	            {
	                var numSlicesToWrite = FOutput.Length;
	                while (numSlicesToWrite > 0)
	                {
	                    var blockSize = Math.Min(numSlicesToWrite, inputBuffer.Length);
	                    inputReader.Read(inputBuffer, 0, blockSize);
	                    offsetReader.Read(offsetBuffer, 0, blockSize);
	                    countReader.Read(countBuffer, 0, blockSize);
	
	                    for (int i = 0; i < blockSize; i++)
	                    {
	                        var source = inputBuffer[i];
	                        var sourceLength = source.Length;
	                        if (sourceLength > 0)
	                        {
	                            var offset = offsetBuffer[i];
	                            var count = countBuffer[i];
	
	                            if (offset < 0 || offset >= sourceLength)
	                            {
	                                offset = VMath.Zmod(offset, sourceLength);
	                            }
	                            if (count < 0)
	                            {
	                                source = source.Reverse();
	                                count = -count;
	                                offset = sourceLength - offset;
	                            }
	                            // offset and count are positive now
	                            if (offset + count > sourceLength)
	                            {
	                                source = source.Cyclic();
	                            }
	
	                            inputBuffer[i] = source.GetRange(offset, count);
	                        }
	                    }
	
	                    numSlicesToWrite -= outputWriter.Write(inputBuffer, 0, blockSize);
	                }
	            }
	        }
	        finally
	        {
	            MemoryPool<IInStream<T>>.PutArray(inputBuffer);
	            MemoryPool<int>.PutArray(offsetBuffer);
	            MemoryPool<int>.PutArray(countBuffer);
	        }
	    }
	}
}
