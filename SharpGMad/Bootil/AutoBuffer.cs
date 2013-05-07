using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bootil
{
    class AutoBuffer
    {
        protected char[] m_pData;
        protected uint m_iSize;
        protected uint m_iPos;
        protected uint m_iWritten;

        public AutoBuffer(int iInitialSize)
        {
            EnsureCapacity((uint)iInitialSize);
        }

        ~AutoBuffer()
        {
            m_pData = new char[]{};
        }

        public void Clear()
        {
            m_iWritten = 0;
            m_iPos = 0;
            m_iSize = 0;

            m_pData = new char[]{};
        }

        public bool EnsureCapacity(uint iSize)
        {
            if (iSize <= m_iSize) { return true; }

            //
            // More than 500mb - we're probably doing something wrong - right??
            //
            if (iSize > 536870912) { return false; }

            if (m_pData == null)
            {
                m_pData = new char[iSize];

                if (m_pData == null)
                {
                    return false;
                }
            }
            else
            {
                char[] pData = new char[iSize];
                m_pData.CopyTo(pData, 0);
                
                if (pData != null)
                {
                    m_pData = pData;
                }
                else
                {
                    pData = new char[iSize];

                    if (pData == null) { return false; }

                    pData.CopyTo(m_pData, 0);
                    m_pData = new char[] { };
                    m_pData = pData;
                }

                if (m_pData == null)
                {
                    return false;
                }
            }

            m_iSize = iSize;
            return true;
        }

        public int WriteString(string str)
        {
            int iWritten = 0;

            for (int i = 0; i < str.Length; i++)
            {
                m_pData[i] = str[i];
                iWritten++;
            }

            m_pData[iWritten + 1] = '0';
            iWritten++;
            return iWritten;
        }

        public string ReadString()
        {
            string str = "";
            int i = 0;

            while (true)
            {
                if (m_iPos + sizeof(char) > m_iSize)
                    break;

                char c = m_pData[i];
                i++;

                if (c == 0) { break; }

                str += c;
            }

            return str;
        }
    }
}
