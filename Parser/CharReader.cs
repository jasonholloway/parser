using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Parser
{
    
    public class CharReader : IEnumerator<char>
    {
        public string Source { get; private set; }

        public char Current { get; private set; }

        public int ReadStart { get; private set; }
        public int ReadLength { get; private set; }


        public int RemainingCount { get; private set; }

        public bool AtEnd { get; private set; }


        public CharReader(string source, int startIndex = 0, int length = -1) {
            Source = source;
            RemainingCount = length < 0 ? Source.Length : length;
            ReadStart = startIndex;
            ReadLength = 0;
            Current = (char)0;
            AtEnd = false;
        }


        public bool MoveNext() {
            ReadStart += ReadLength;
            ReadLength = 0;

            if(RemainingCount > 0) {
                RemainingCount--;
                if(RemainingCount < 0) throw new InvalidOperationException("Read past end of string!");

                Current = Source[ReadStart + ReadLength++];

                if(Current == '%') {
                    RemainingCount -= 2;
                    if(RemainingCount < 0) throw new InvalidOperationException("Read past end of string!");

                    var nibble1 = Source[ReadStart + ReadLength++];
                    var nibble2 = Source[ReadStart + ReadLength++];
                    Current = (char)((nibble1.DecodeAsHex() << 4) + (nibble2.DecodeAsHex()));
                }

                return true;
            }
            else {
                Current = (char)0;
                AtEnd = true;
                return false;
            }
        }

        public void Reset() {
            ReadStart = ReadLength = 0;
            Current = (char)0;
        }

        public void Dispose() { }

        object IEnumerator.Current => Current;
    }




}
