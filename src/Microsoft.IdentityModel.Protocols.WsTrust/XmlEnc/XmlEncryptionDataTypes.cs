//------------------------------------------------------------------------------
//
// Copyright (c) Microsoft Corporation.
// All rights reserved.
//
// This code is licensed under the MIT License.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files(the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions :
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//------------------------------------------------------------------------------

namespace Microsoft.IdentityModel.Protocols.XmlEnc
{
    /// <summary>
    /// Constants for XML Encryption DataTypes.
    /// <para>see: https://www.w3.org/TR/xmlenc-core1/ </para>
    /// </summary>
    internal abstract class XmlEncryptionDataTypes
    {
        /// <summary>
        /// Gets DataTypes for XML Encryption 1.1.
        /// </summary>
        public static XmlEncryption11DataTypes XmlEnc11 { get; } = new XmlEncryption11DataTypes();

        /// <summary>
        /// Gets Content DataType type for XML Encryption
        /// </summary>
        public string Content { get; protected set; }

        /// <summary>
        /// Gets Content DataType type for XML Encryption
        /// </summary>
        public string Element { get; protected set; }
    }

    /// <summary>
    /// Provides DataTypes for XML Encryption 1.1.
    /// </summary>
    internal class XmlEncryption11DataTypes : XmlEncryptionDataTypes
    {
        /// <summary>
        /// Instantiates DataTypes for XML Encryption 1.1.
        /// </summary>
        public XmlEncryption11DataTypes()
        {
            Content = "http://www.w3.org/2001/04/xmlenc#Content";
            Element  = "http://www.w3.org/2001/04/xmlenc#Element";
        }
    }
}