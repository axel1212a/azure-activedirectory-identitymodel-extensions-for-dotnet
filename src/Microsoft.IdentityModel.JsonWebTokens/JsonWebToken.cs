﻿//------------------------------------------------------------------------------
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

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;

#if NET45
using Microsoft.IdentityModel.Json;
using Microsoft.IdentityModel.Json.Linq;
using JsonClaimSet = Microsoft.IdentityModel.JsonWebTokens.JsonClaimSet45;
#else
using System.Text.Json;
#endif

namespace Microsoft.IdentityModel.JsonWebTokens
{
    /// <summary>
    /// A <see cref="SecurityToken"/> designed for representing a JSON Web Token (JWT). 
    /// </summary>
    public class JsonWebToken : SecurityToken, IJsonClaimSet
    {
        private ClaimsIdentity _claimsIdentity;
        private bool _wasClaimsIdentitySet;

        private string _act;
        private string _alg;
        private IList<string> _audiences;
        private string _authenticationTag;
        private string _ciphertext;
        private string _cty;
        private string _enc;
        private string _encodedHeader;
        private string _encodedPayload;
        private string _encodedSignature;
        private string _encryptedKey;
        private DateTime? _iat;
        private string _id;
        private string _initializationVector;
        private string _iss;
        private string _kid;
        private string _sub;
        private string _typ;
        private DateTime? _validFrom;
        private DateTime? _validTo;
        private string _x5t;
        private string _zip;

        /// <summary>
        /// Initializes a new instance of <see cref="JsonWebToken"/> from a string in JWS or JWE Compact serialized format.
        /// </summary>
        /// <param name="jwtEncodedString">A JSON Web Token that has been serialized in JWS or JWE Compact serialized format.</param>
        /// <exception cref="ArgumentNullException">'jwtEncodedString' is null or empty.</exception>
        /// <exception cref="ArgumentException">'jwtEncodedString' is not in JWS or JWE Compact serialization format.</exception>
        /// <remarks>
        /// see: https://datatracker.ietf.org/doc/html/rfc7519 (JWT)
        /// see: https://datatracker.ietf.org/doc/html/rfc7515 (JWS)
        /// see: https://datatracker.ietf.org/doc/html/rfc7516 (JWE)
        /// <para>
        /// The contents of the returned <see cref="JsonWebToken"/> have not been validated, the JSON Web Token is simply decoded. Validation can be accomplished using the validation methods in <see cref="JsonWebTokenHandler"/>
        /// </para>
        /// </remarks>
        public JsonWebToken(string jwtEncodedString)
        {
            if (string.IsNullOrEmpty(jwtEncodedString))
                throw LogHelper.LogExceptionMessage(new ArgumentNullException(nameof(jwtEncodedString)));

            ReadToken(jwtEncodedString);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonWebToken"/> class where the header contains the crypto algorithms applied to the encoded header and payload.
        /// </summary>
        /// <param name="header">A string containing JSON which represents the cryptographic operations applied to the JWT and optionally any additional properties of the JWT.</param>
        /// <param name="payload">A string containing JSON which represents the claims contained in the JWT. Each claim is a JSON object of the form { Name, Value }.</param>
        /// <remarks>
        /// see: https://datatracker.ietf.org/doc/html/rfc7519 (JWT)
        /// see: https://datatracker.ietf.org/doc/html/rfc7515 (JWS)
        /// see: https://datatracker.ietf.org/doc/html/rfc7516 (JWE)
        /// <para>
        /// The contents of the returned <see cref="JsonWebToken"/> have not been validated, the JSON Web Token is simply decoded. Validation can be accomplished using the validation methods in <see cref="JsonWebTokenHandler"/>
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">'header' is null.</exception>
        /// <exception cref="ArgumentNullException">'payload' is null.</exception>
        public JsonWebToken(string header, string payload)
        {
            if (string.IsNullOrEmpty(header))
                throw LogHelper.LogArgumentNullException(nameof(header));

            if (string.IsNullOrEmpty(payload))
                throw LogHelper.LogArgumentNullException(nameof(payload));

            try
            {
                Header = new JsonClaimSet(header);
            }
            catch (Exception ex)
            {
                throw LogHelper.LogExceptionMessage(new ArgumentException(LogHelper.FormatInvariant(LogMessages.IDX14301, header), ex));
            }

            try
            {
                Payload = new JsonClaimSet(payload);
            }
            catch (Exception ex)
            {
                throw LogHelper.LogExceptionMessage(new ArgumentException(LogHelper.FormatInvariant(LogMessages.IDX14302, payload), ex));
            }
        }

        internal string ActualIssuer { get; set; }

        internal ClaimsIdentity ActorClaimsIdentity { get; set; }


        /// <summary>
        /// Gets the AuthenticationTag from the original raw data of this instance when it was created.
        /// </summary>
        /// <remarks>
        /// Contains the results of a Authentication Encryption with Associated Data (AEAD).
        /// see: https://datatracker.ietf.org/doc/html/rfc7516#section-2
        /// <para>
        /// If this JWT is not encrypted with an algorithms that uses an Authentication Tag, an empty string will be returned.
        /// </para>
        /// </remarks>
        public string AuthenticationTag
        {
            get
            {
                if (_authenticationTag == null)
                {

                    _authenticationTag = AuthenticationTagBytes == null ? string.Empty : UTF8Encoding.UTF8.GetString(AuthenticationTagBytes);
                }

                return _authenticationTag;

            }
        }

        /// <summary>
        ///
        /// </summary>
        internal byte[] AuthenticationTagBytes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the Ciphertext representing the encrypted JWT in the original raw data.
        /// </summary>
        /// <remarks>
        /// When decrypted using values in the JWE header will contain the plaintext payload.
        /// see: https://datatracker.ietf.org/doc/html/rfc7516#section-2
        /// <para>
        /// If this JWT is not encrypted, an empty string will be returned.
        /// </para>
        /// </remarks>
        public string Ciphertext
        {
            get
            {
                if (_ciphertext == null)
                    _ciphertext = CipherTextBytes == null ? string.Empty : UTF8Encoding.UTF8.GetString(CipherTextBytes);

                return _ciphertext;
            }
        }

        /// <summary>
        ///
        /// </summary>
        internal byte[] CipherTextBytes
        {
            get;
            set;
        }

        internal ClaimsIdentity ClaimsIdentity
        {
            get
            {
                if (!_wasClaimsIdentitySet)
                {
                    _wasClaimsIdentitySet = true;
                    string actualIssuer = ActualIssuer ?? Issuer;

                    foreach (Claim jclaim in Claims)
                    {
                        string claimType = jclaim.Type;
                        if (claimType == ClaimTypes.Actor)
                        {
                            if (_claimsIdentity.Actor != null)
                                throw LogHelper.LogExceptionMessage(new InvalidOperationException(LogHelper.FormatInvariant(LogMessages.IDX14112, LogHelper.MarkAsNonPII(JwtRegisteredClaimNames.Actort), jclaim.Value)));

#pragma warning disable CA1031 // Do not catch general exception types
                            try
                            {
                                JsonWebToken actorToken = new JsonWebToken(jclaim.Value);
                                _claimsIdentity.Actor = ActorClaimsIdentity;
                            }
                            catch
                            {

                            }
#pragma warning restore CA1031 // Do not catch general exception types
                        }

                        if (jclaim.Properties.Count == 0)
                        {
                            _claimsIdentity.AddClaim(new Claim(claimType, jclaim.Value, jclaim.ValueType, actualIssuer, actualIssuer, _claimsIdentity));
                        }
                        else
                        {
                            Claim claim = new Claim(claimType, jclaim.Value, jclaim.ValueType, actualIssuer, actualIssuer, _claimsIdentity);

                            foreach (var kv in jclaim.Properties)
                                claim.Properties[kv.Key] = kv.Value;

                            _claimsIdentity.AddClaim(claim);
                        }
                    }
                }

                return _claimsIdentity;
            }

            set
            {
                _claimsIdentity = value;
            }
        }

        internal int Dot1 { get; set; }

        internal int Dot2 { get; set; }

        internal int Dot3 { get; set; }

        internal int Dot4 { get; set; }

        /// <summary>
        /// Gets the EncodedHeader from the original raw data of this instance when it was created.
        /// </summary>
        /// <remarks>
        /// The original Base64UrlEncoded string of the JWT header.
        /// </remarks>
        public string EncodedHeader
        {
            get
            {
                // TODO - need to account for JWE
                if (_encodedHeader == null)
                {
                    if (EncodedToken != null)
                        _encodedHeader = EncodedToken.Substring(0, Dot1);
                    else
                        _encodedHeader = string.Empty;
                }

                return _encodedHeader;
            }
        }

        /// <summary>
        /// Gets the Encrypted Content Encryption Key.
        /// </summary>
        /// <remarks>
        /// For some algorithms this value may be null even though the JWT was encrypted.
        /// see: https://datatracker.ietf.org/doc/html/rfc7516#section-2
        /// <para>
        /// If not found, an empty string is returned.
        /// </para>
        /// </remarks>
        public string EncryptedKey
        {
            get
            {
                if (_encryptedKey == null)
                    _encryptedKey = EncryptedKeyBytes == null ? string.Empty : UTF8Encoding.UTF8.GetString(EncryptedKeyBytes);

                return _encryptedKey;
            }
        }

        internal byte[] EncryptedKeyBytes { get; set; }

        /// <summary>
        /// Gets the EncodedPayload from the original raw data of this instance when it was created.
        /// </summary>
        /// <remarks>
        /// The original Base64UrlEncoded of the JWT payload, for JWE this will an empty string.
        /// </remarks>
        public string EncodedPayload
        {
            get
            {
                if (_encodedPayload == null)
                {
                    if (EncodedToken != null)
                        _encodedPayload = IsEncrypted ? string.Empty : EncodedToken.Substring(Dot1 + 1, Dot2 - Dot1 - 1);
                    else
                        _encodedPayload = string.Empty;
                }

                return _encodedPayload;
            }
        }

        /// <summary>
        /// Gets the EncodedSignature from the original raw data of this instance when it was created.
        /// </summary>
        /// <remarks>
        /// The original Base64UrlEncoded of the JWT signature.
        /// If the JWT was not signed or a JWE, an empty string is returned.
        /// </remarks>
        public string EncodedSignature
        {
            get
            {
                if (_encodedSignature == null)
                {
                    if (EncodedToken != null)
                        _encodedSignature = IsEncrypted ? string.Empty : EncodedToken.Substring(Dot2 + 1, EncodedToken.Length - Dot2 - 1);
                    else
                        _encodedSignature = string.Empty;
                }

                return _encodedSignature;
            }
        }

        /// <summary>
        /// Gets the original raw data of this instance when it was created.
        /// </summary>
        /// <remarks>
        /// The original Base64UrlEncoded of the JWT.
        /// </remarks>
        public string EncodedToken { get; private set; }

        internal bool HasPayloadClaim(string claimName)
        {
            return Payload.HasClaim(claimName);
        }

        internal JsonClaimSet Header { get; set; }

        internal byte[] HeaderAsciiBytes { get; set; }

        internal byte[] InitializationVectorBytes { get; set; }

        /// <summary>
        /// Gets the Initialization Vector used when encrypting the plaintext.
        /// </summary>
        /// <remarks>
        /// see: https://datatracker.ietf.org/doc/html/rfc7516#appendix-A.1.4
        /// <para>
        /// Some algorithms may not use an Initialization Vector.
        /// If not found an empty string is returned.
        /// </para>
        /// </remarks>
        public string InitializationVector
        {
            get
            {
                if (InitializationVectorBytes == null)
                    _initializationVector = InitializationVectorBytes == null ? string.Empty : UTF8Encoding.UTF8.GetString(InitializationVectorBytes);

                return _initializationVector;
            }
        }

        /// <summary>
        /// Gets the <see cref="JsonWebToken"/> associated with this instance.
        /// </summary>
        /// <remarks>
        /// see: https://datatracker.ietf.org/doc/html/rfc7516#section-2
        /// For encrypted tokens {JWE}, this represents the JWT that was encrypted.
        /// <para>
        /// If the JWT is not encrypted, this value will be null.
        /// </para>
        /// </remarks>
        public JsonWebToken InnerToken { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsEncrypted { get => CipherTextBytes != null; }

        /// <summary>
        /// TODO - do we need to set this or use IsEncrypted model.
        /// </summary>
        public bool IsSigned { get; internal set; }

        /// <summary>
        ///
        /// </summary>
        internal JsonClaimSet Payload { get; set; }

        /// <summary>
        /// Not implemented.
        /// </summary>
        public override SecurityKey SecurityKey { get; }

        /// <summary>
        /// Gets or sets the <see cref="SecurityKey"/> that was used to sign this token.
        /// </summary>
        /// <remarks>
        /// If the JWT was not signed or validated, this value will be null.
        /// </remarks>
        public override SecurityKey SigningKey { get; set; }

        /// <summary>
        ///
        /// </summary>
        internal byte[] MessageBytes{ get; set; }

        internal int NumberOfDots { get; set; }

        private void ReadToken(string encodedJson)
        {
            // JWT must have 2 dots

            StringSegment jsonSegments = new StringSegment(encodedJson);
            var segments = jsonSegments.Split('.');

            if (segments.Count != JwtConstants.JwsSegmentCount && segments.Count != JwtConstants.JweSegmentCount)
                throw LogHelper.LogExceptionMessage(new ArgumentException(LogHelper.FormatInvariant(LogMessages.IDX14100, encodedJson)));

            Dot1 = segments[0].EndIndex;
            Dot2 = segments[1].EndIndex;
            Dot3 = segments[2].EndIndex;

            var header = segments[0];

            // JWS
            if (segments.Count == 3)
            {
                var payload = segments[1];
                var signature = segments[2];
                IsSigned = !signature.IsEmpty;
                try
                {
#if NET45
                    SignatureBytes = Base64UrlEncoder.UnsafeDecode(signature);
                    Header = new JsonClaimSet(Base64UrlEncoder.UnsafeDecode(header));
#else
                    Header = new JsonClaimSet(JwtTokenUtilities.GetJsonDocumentFromBase64UrlEncodedString(header));
#endif
                }
                catch (Exception ex)
                {
                    throw LogHelper.LogExceptionMessage(new ArgumentException(LogHelper.FormatInvariant(LogMessages.IDX14102, header, encodedJson), ex));
                }

                try
                {
#if NET45
                    MessageBytes = Encoding.UTF8.GetBytes(encodedJson.ToCharArray(0, header.Length + payload.Length + 1));
                    Payload = new JsonClaimSet(Base64UrlEncoder.UnsafeDecode(payload));
#else
                    Payload = new JsonClaimSet(JwtTokenUtilities.GetJsonDocumentFromBase64UrlEncodedString(payload));
#endif
                }
                catch (Exception ex)
                {
                    throw LogHelper.LogExceptionMessage(new ArgumentException(LogHelper.FormatInvariant(LogMessages.IDX14101, payload, encodedJson), ex));
                }
            }
            else //JWE
            {
                Dot4 = segments[3].EndIndex;

                // header cannot be empty
                if (header.IsEmpty)
                    throw LogHelper.LogExceptionMessage(new ArgumentException(LogHelper.FormatInvariant(LogMessages.IDX14307, encodedJson)));

                HeaderAsciiBytes = Encoding.ASCII.GetBytes(encodedJson.ToCharArray(0, header.Length));
                try
                {
                    Header = new JsonClaimSet(Base64UrlEncoder.UnsafeDecode(header));
                }
                catch (Exception ex)
                {
                    throw LogHelper.LogExceptionMessage(new ArgumentException(LogHelper.FormatInvariant(LogMessages.IDX14102, header, encodedJson), ex));
                }

                // dir does not have any key bytes
                var encryptedKey = segments[1];
                if (!encryptedKey.IsEmpty)
                {
                    EncryptedKeyBytes = Base64UrlEncoder.UnsafeDecode(encryptedKey);
                    _encryptedKey = encryptedKey.ToString();
                }
                else
                {
                    _encryptedKey = string.Empty;
                }

                var initializationVector = segments[2];
                if (initializationVector.IsEmpty)
                    throw LogHelper.LogExceptionMessage(new ArgumentException(LogHelper.FormatInvariant(LogMessages.IDX14308, encodedJson)));

                try
                {
                    InitializationVectorBytes = Base64UrlEncoder.UnsafeDecode(initializationVector);
                }
                catch (Exception ex)
                {
                    throw LogHelper.LogExceptionMessage(new ArgumentException(LogHelper.FormatInvariant(LogMessages.IDX14309, encodedJson, encodedJson), ex));
                }

                var authenticationTag = segments[4];
                if (authenticationTag.IsEmpty)
                    throw LogHelper.LogExceptionMessage(new ArgumentException(LogHelper.FormatInvariant(LogMessages.IDX14310, encodedJson)));

                try
                {
                    AuthenticationTagBytes = Base64UrlEncoder.UnsafeDecode(authenticationTag);
                }
                catch (Exception ex)
                {
                    throw LogHelper.LogExceptionMessage(new ArgumentException(LogHelper.FormatInvariant(LogMessages.IDX14311, encodedJson, encodedJson), ex));
                }

                var cipherText = segments[3];
                if (cipherText.IsEmpty)
                    throw LogHelper.LogExceptionMessage(new ArgumentException(LogHelper.FormatInvariant(LogMessages.IDX14306, encodedJson)));

                try
                {
                    CipherTextBytes = Base64UrlEncoder.UnsafeDecode(cipherText);
                }
                catch (Exception ex)
                {
                    throw LogHelper.LogExceptionMessage(new ArgumentException(LogHelper.FormatInvariant(LogMessages.IDX14312, encodedJson, encodedJson), ex));
                }
            }

            EncodedToken = encodedJson;
        }

        /// <summary>
        ///
        /// </summary>
        internal byte[] SignatureBytes { get; set; }

#region Claims
        /// <summary>
        /// Gets the 'value' of the 'actort' claim the payload.
        /// </summary>
        /// <remarks>
        /// If the 'actort' claim is not found, an empty string is returned.
        /// </remarks>
        public string Actor
        {
            get
            {
                if (_act == null)
                {
                    _act = (InnerToken == null) ? Payload.GetStringValue(JwtRegisteredClaimNames.Actort) : InnerToken.Payload.GetStringValue(JwtRegisteredClaimNames.Actort);
                }

                return _act;
            }
        }

        /// <summary>
        /// Gets the 'value' of the 'alg' claim from the header.
        /// </summary>
        /// <remarks>
        /// Identifies the cryptographic algorithm used to encrypt or determine the value of the Content Encryption Key.
        /// Applicable to an encrypted JWT {JWE}.
        /// see: https://datatracker.ietf.org/doc/html/rfc7516#section-4.1.1
        /// <para>
        /// If the 'alg' claim is not found, an empty string is returned.
        /// </para>
        /// </remarks>
        public string Alg
        {
            get
            {
                if (_alg == null)
                {
                    _alg = Header.GetStringValue(JwtHeaderParameterNames.Alg);
                }

                return _alg;
            }
        }

        /// <summary>
        /// Gets the list of 'aud' claims from the payload.
        /// </summary>
        /// <remarks>
        /// Identifies the recipients that the JWT is intended for.
        /// see: https://datatracker.ietf.org/doc/html/rfc7519#section-4.1.3
        /// <para>
        /// If the 'aud' claim is not found, enumeration will be empty.
        /// </para>
        /// </remarks>
        public IEnumerable<string> Audiences
        {
            get
            {
                if (_audiences == null)
                {
                    _audiences = new List<string>();
#if NET45
                    if (Payload.TryGetValue(JwtRegisteredClaimNames.Aud, out JToken value))
                    {
                        if (value.Type is JTokenType.String)
                            _audiences = new List<string> { value.ToObject<string>() };
                        else if (value.Type is JTokenType.Array)
                            _audiences = value.ToObject<List<string>>();
                    }
#else
                    if (Payload.TryGetValue(JwtRegisteredClaimNames.Aud, out JsonElement audiences))
                    {
                        if (audiences.ValueKind == JsonValueKind.String)
                            _audiences = new List<string> { audiences.GetString() };

                        if (audiences.ValueKind == JsonValueKind.Array)
                        {
                            foreach (JsonElement jsonElement in audiences.EnumerateArray())
                                _audiences.Add(jsonElement.ToString());
                        }
                    }
#endif
                }

                return _audiences;
            }
        }

        internal override IEnumerable<Claim> CreateClaims(string issuer)
        {
            if (InnerToken != null)
                return InnerToken.CreateClaims(issuer);

            return Payload.CreateClaims(issuer);
        }

        /// <summary>
        /// Gets a <see cref="IEnumerable{Claim}"/> where each claim in the JWT { name, value } is returned as a <see cref="Claim"/>.
        /// </summary>
        /// <remarks>
        /// A <see cref="Claim"/> requires each value to be represented as a string. If the value was not a string, then <see cref="Claim.Type"/> contains the json type.
        /// <see cref="JsonClaimValueTypes"/> and <see cref="ClaimValueTypes"/> to determine the json type.
        /// </remarks>
        public virtual IEnumerable<Claim> Claims
        {
            get
            {
                if (InnerToken != null)
                    return InnerToken.Claims;

                return Payload.Claims(Issuer ?? ClaimsIdentity.DefaultIssuer);

            }
        }

        /// <summary>
        /// Gets the 'value' of the 'cty' claim from the header.
        /// </summary>
        /// <remarks>
        /// Used by JWS applications to declare the media type[IANA.MediaTypes] of the secured content (the payload).
        /// see: https://datatracker.ietf.org/doc/html/rfc7516#section-4.1.12 (JWE)
        /// see: https://datatracker.ietf.org/doc/html/rfc7515#section-4.1.10 (JWS)
        /// <para>
        /// If the 'cty' claim is not found, an empty string is returned.
        /// </para>
        /// </remarks>
        public string Cty
        {
            get
            {
                if (_cty == null)
                    _cty = Header.GetStringValue(JwtHeaderParameterNames.Cty);

                return _cty;
            }
        }

        /// <summary>
        /// Gets the 'value' of the 'enc' claim from the header.
        /// </summary>
        /// <remarks>
        /// Identifies the content encryption algorithm used to perform authenticated encryption
        /// on the plaintext to produce the ciphertext and the Authentication Tag.
        /// see: https://datatracker.ietf.org/doc/html/rfc7516#section-4.1.2
        /// </remarks>
        public string Enc
        {
            get
            {
                if (_enc == null)
                    _enc = Header.GetStringValue(JwtHeaderParameterNames.Enc);

                return _enc;
            }
        }

        /// <summary>
        /// Gets a <see cref="Claim"/> representing the { key, 'value' } pair corresponding to the provided <paramref name="key"/>.
        /// </summary>
        /// <remarks>
        /// A <see cref="Claim"/> requires each value to be represented as a string. If the value was not a string, then <see cref="Claim.Type"/> contains the json type.
        /// <see cref="JsonClaimValueTypes"/> and <see cref="ClaimValueTypes"/> to determine the json type.
        /// <para>
        /// If the key has no corresponding value, this method will throw.
        /// </para>
        /// </remarks>
        public Claim GetClaim(string key)
        {
            return Payload.GetClaim(key, Issuer ?? ClaimsIdentity.DefaultIssuer);
        }

        /// <summary>
        /// Gets the 'value' corresponding to key from the JWT header transformed as type 'T'.
        /// </summary>
        /// <remarks>
        /// The expectation is that the 'value' corresponds to a type are expected in a JWT token.
        /// The 5 basic types: number, string, true / false, nil, array (of basic types).
        /// This is not a general purpose translation layer for complex types.
        /// </remarks>
        /// <returns>The value as <typeparamref name="T"/>.</returns>
        /// <exception cref="ArgumentException">if claim is not found or a transformation to <typeparamref name="T"/> cannot be made.</exception>
        public T GetHeaderValue<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw LogHelper.LogArgumentNullException(nameof(key));

            return Header.GetValue<T>(key);
        }

        /// <summary>
        /// Gets the 'value' corresponding to key from the JWT payload transformed as type 'T'.
        /// </summary>
        /// <remarks>
        /// The expectation is that the 'value' corresponds to a type are expected in a JWT token.
        /// The 5 basic types: number, string, true / false, nil, array (of basic types).
        /// This is not a general purpose translation layer for complex types.
        /// </remarks>
        /// <returns>The value as <typeparamref name="T"/>.</returns>
        /// <exception cref="ArgumentException">if claim is not found or a transformation to <typeparamref name="T"/> cannot be made.</exception>
        public T GetPayloadValue<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw LogHelper.LogArgumentNullException(nameof(key));

            if (typeof(T).Equals(typeof(Claim)))
                return (T)(object)GetClaim(key);

            return Payload.GetValue<T>(key);
        }

        /// <summary>
        /// Gets the 'value' of the 'jti' claim from the payload.
        /// </summary>
        /// <remarks>
        /// Provides a unique identifier for the JWT.
        /// see: https://datatracker.ietf.org/doc/html/rfc7519#section-4.1.7
        /// <para>
        /// If the 'jti' claim is not found, an empty string is returned.
        /// </para>
        /// </remarks>
        public override string Id
        {
            get
            {
                if (_id == null)
                    _id = Payload.GetStringValue(JwtRegisteredClaimNames.Jti);

                return _id;
            }
        }

        /// <summary>
        /// Gets the 'value' of the 'iat' claim converted to a <see cref="DateTime"/> from the payload.
        /// </summary>
        /// <remarks>
        /// Identifies the time at which the JWT was issued.
        /// see: https://datatracker.ietf.org/doc/html/rfc7519#section-4.1.6
        /// <para>
        /// If the 'iat' claim is not found, then <see cref="DateTime.MinValue"/> is returned.
        /// </para>
        /// </remarks>
        public DateTime IssuedAt
        {
            get
            {
                if (_iat == null)
                    _iat = Payload.GetDateTime(JwtRegisteredClaimNames.Iat);

                return _iat.Value;
            }
        }

        /// <summary>
        /// Gets the 'value' of the 'iss' claim from the payload.
        /// </summary>
        /// <remarks>
        /// Identifies the principal that issued the JWT.
        /// see: https://datatracker.ietf.org/doc/html/rfc7519#section-4.1.1
        /// <para>
        /// If the 'iss' claim is not found, an empty string is returned.
        /// </para>
        /// </remarks>
        public override string Issuer
        {
            get
            {
                if (_iss == null)
                    _iss = Payload.GetStringValue(JwtRegisteredClaimNames.Iss);

                return _iss;
            }
        }

        /// <summary>
        /// Gets the 'value' of the 'kid' claim from the header.
        /// </summary>
        /// <remarks>
        /// 'kid'is a hint indicating which key was used to secure the JWS.
        /// see: https://datatracker.ietf.org/doc/html/rfc7515#section-4.1.4 (JWS)
        /// see: https://datatracker.ietf.org/doc/html/rfc7516#section-4.1.6 (JWE)
        /// <para>
        /// If the 'kid' claim is not found, an empty string is returned.
        /// </para>
        /// </remarks>
        public string Kid
        {
            get
            {
                if (_kid == null)
                    _kid = Header.GetStringValue(JwtHeaderParameterNames.Kid);

                return _kid;
            }
        }

        /// <summary>
        /// Gets the 'value' of the 'sub' claim from the payload.
        /// </summary>
        /// <remarks>
        /// see: https://datatracker.ietf.org/doc/html/rfc7519#section-4.1.2
        /// Identifies the principal that is the subject of the JWT.
        /// <para>
        /// If the 'sub' claim is not found, an empty string is returned.
        /// </para>
        /// </remarks>
        public string Subject
        {
            get
            {
                if (_sub == null)
                    _sub = Payload.GetStringValue(JwtRegisteredClaimNames.Sub);

                return _sub;
            }
        }

        /// <summary>
        /// Try to get a <see cref="Claim"/> representing the { key, 'value' } pair corresponding to the provided <paramref name="key"/>.
        /// </summary>
        /// <remarks>
        /// A <see cref="Claim"/> requires each value to be represented as a string. If the value was not a string, then <see cref="Claim.Type"/> contains the json type.
        /// <see cref="JsonClaimValueTypes"/> and <see cref="ClaimValueTypes"/> to determine the json type.
        /// </remarks>
        /// <returns>true if successful, false otherwise.</returns>
        public bool TryGetClaim(string key, out Claim value)
        {
            return Payload.TryGetClaim(key, Issuer ?? ClaimsIdentity.DefaultIssuer, out value);
        }

        /// <summary>
        /// Tries to get the value
        /// </summary>
        /// <remarks>
        /// The expectation is that the 'value' corresponds to a type expected in a JWT token.
        /// </remarks>
        /// <returns>true if successful, false otherwise.</returns>
        public bool TryGetValue<T>(string key, out T value)
        {
            if (string.IsNullOrEmpty(key))
            {
                value = default;
                return false;
            }

            return Payload.TryGetValue(key, out value);
        }

        /// <summary>
        /// Tries to get the value corresponding to the provided key from the JWT header { key, 'value' }.
        /// </summary>
        /// <remarks>
        /// The expectation is that the 'value' corresponds to a type expected in a JWT token.
        /// The 5 basic types: number, string, true / false, nil, array (of basic types).
        /// This is not a general purpose translation layer for complex types.
        /// </remarks>
        /// <returns>true if successful, false otherwise.</returns>
        public bool TryGetHeaderValue<T>(string key, out T value)
        {
            if (string.IsNullOrEmpty(key))
            {
                value = default;
                return false;
            }

            return Header.TryGetValue(key, out value);
        }

        /// <summary>
        /// Try to get the 'value' corresponding to key from the JWT payload transformed as type 'T'.
        /// </summary>
        /// <remarks>
        /// The expectation is that the 'value' corresponds to a type are expected in a JWT token.
        /// The 5 basic types: number, string, true / false, nil, array (of basic types).
        /// This is not a general purpose translation layer for complex types.
        /// </remarks>
        /// <returns>true if successful, false otherwise.</returns>
        public bool TryGetPayloadValue<T>(string key, out T value)
        {
            if (string.IsNullOrEmpty(key))
            {
                value = default;
                return false;
            }

            if (typeof(T).Equals(typeof(Claim)))
            {
                bool foundClaim = TryGetClaim(key, out var claim);
                value = (T)(object)claim;
                return foundClaim;
            }

            return Payload.TryGetValue(key, out value);
        }

        /// <summary>
        /// Gets the 'value' of the 'typ' claim from the header.
        /// </summary>
        /// <remarks>
        /// Is used by JWT applications to declare the media type.
        /// see: https://datatracker.ietf.org/doc/html/rfc7519#section-5.1
        /// <para>
        /// If the 'typ' claim is not found, an empty string is returned.
        /// </para>
        /// </remarks>
        public string Typ
        {
            get
            {
                if (_typ == null)
                    _typ = Header.GetStringValue(JwtHeaderParameterNames.Typ);

                return _typ;
            }
        }

        /// <summary>
        /// Gets the 'value' of the 'x5t' claim from the header.
        /// </summary>
        /// <remarks>
        /// Is the base64url-encoded SHA-1 thumbprint(a.k.a.digest) of the DER encoding of the X.509 certificate used to sign this token.
        /// see : https://datatracker.ietf.org/doc/html/rfc7515#section-4.1.7
        /// <para>
        /// If the 'x5t' claim is not found, an empty string is returned.
        /// </para>
        /// </remarks>
        public string X5t
        {
            get
            {
                if (_x5t == null)
                    _x5t = Header.GetStringValue(JwtHeaderParameterNames.X5t);

                return _x5t;
            }
        }

        /// <summary>
        /// Gets the 'value' of the 'nbf' claim converted to a <see cref="DateTime"/> from the payload.
        /// </summary>
        /// <remarks>
        /// Identifies the time before which the JWT MUST NOT be accepted for processing.
        /// see: https://datatracker.ietf.org/doc/html/rfc7519#section-4.1.5
        /// <para>
        /// If the 'nbf' claim is not found, then <see cref="DateTime.MinValue"/> is returned.
        /// </para>
        /// </remarks>
        public override DateTime ValidFrom
        {
            get
            {
                if (_validFrom == null)
                    _validFrom = Payload.GetDateTime(JwtRegisteredClaimNames.Nbf);

                return _validFrom.Value;
            }
        }

        /// <summary>
        /// Gets the 'value' of the 'exp' claim converted to a <see cref="DateTime"/> from the payload.
        /// </summary>
        /// <remarks>
        /// Identifies the expiration time on or after which the JWT MUST NOT be accepted for processing.
        /// see: https://datatracker.ietf.org/doc/html/rfc7519#section-4.1.4
        /// <para>
        /// If the 'exp' claim is not found, then <see cref="DateTime.MinValue"/> is returned.
        /// </para>
        /// </remarks>
        public override DateTime ValidTo
        {
            get
            {
                if (_validTo == null)
                    _validTo = Payload.GetDateTime(JwtRegisteredClaimNames.Exp);

                return _validTo.Value;
            }
        }

        /// <summary>
        /// Gets the 'value' of the 'zip' claim from the header.
        /// </summary>
        /// <remarks>
        /// The "zip" (compression algorithm) applied to the plaintext before encryption, if any.
        /// see: https://datatracker.ietf.org/doc/html/rfc7516#section-4.1.3
        /// <para>
        /// If the 'zip' claim is not found, an empty string is returned.
        /// </para>
        /// </remarks>
        public string Zip
        {
            get
            {
                if (_zip == null)
                    _zip = Header.GetStringValue(JwtHeaderParameterNames.Zip);

                return _zip;
            }
        }

#endregion
    }
}
