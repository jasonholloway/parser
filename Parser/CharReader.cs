using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Parser
{
    
    public class CharReader : IEnumerator<char>
    {
        public string Source { get; private set; }
        public int End { get; private set; }

        public char Current { get; private set; }

        public int ReadStart { get; private set; }
        public int ReadLength { get; private set; }
        

        public CharReader(string source, int startIndex = 0, int length = -1) {
            Source = source;
            End = length >= 0 ? startIndex + length : source.Length;
            ReadStart = startIndex;
            ReadLength = 0;
            Current = (char)0;
        }


        public bool MoveNext() {
            ReadStart += ReadLength;
            ReadLength = 0;

            if(ReadStart < End) 
            {
                Current = Source[ReadStart + ReadLength++];

                if(Current == '%') {
                    var nibble1 = Source[ReadStart + ReadLength++];
                    var nibble2 = Source[ReadStart + ReadLength++];
                    Current = (char)((nibble1.DecodeAsHex() << 4) + (nibble2.DecodeAsHex()));
                }

                return true;
            }
            else {
                Current = (char)0;
                return false;
            }
        }

        public void Reset() {
            ReadStart = ReadLength = 0;
            Current = (char)0;
        }


        public CharReader Clone()
            => (CharReader)MemberwiseClone();            



        public void Dispose() { }

        object IEnumerator.Current => Current;
    }




}
