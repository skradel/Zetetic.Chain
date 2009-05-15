using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Reflection;
using NLog;

namespace Zetetic.Chain.Xml
{
    public abstract class XmlConfig
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public abstract ICommand ResolveInternals();

        [XmlIgnore]
        public List<string> MissingProperties = new List<string>();

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlElement("add")]
        public List<XmlConfigProperty> Properties = new List<XmlConfigProperty>();

        public static XmlConfig CreateFromICommand(ICommand cmd)
        {
            if (cmd is IChain)
            {
                XmlChain x = new XmlChain(cmd.Name);
                foreach (ICommand c in ((IChain)cmd).Commands())
                {
                    x.Commands.Add(XmlConfig.CreateFromICommand(c));
                }
                SerializeProperties(cmd, x);
                return x;
            }
            else
            {
                XmlCommand x = new XmlCommand(cmd.Name, cmd.GetType().AssemblyQualifiedName);
                SerializeProperties(cmd, x);
                return x;
            }
        }

        private string GetPropValue(string key)
        {
            foreach (XmlConfigProperty p in this.Properties)
            {
                if (p.Key == key)
                    return p.Value;
            }
            return null;
        }

        /// <summary>
        /// Deserialize the properties of the current XmlChain / XmlCommand and try to set
        /// the writeable properties of the output ICommand
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        /// <exception cref="ChainXmlSerializationException">If ChainRequired property is absent</exception>
        protected ICommand DeserializeProperties(ICommand target)
        {
            Type t = target.GetType();
            foreach (PropertyInfo pi in target.GetType().GetProperties())
            {
                if (pi.CanWrite)
                {
                    string val = this.GetPropValue(pi.Name);

                    if (string.IsNullOrEmpty(val))
                    {
                        if (IsChainRequired(pi))
                            throw new ChainXmlSerializationException("Missing ChainRequired property " + pi.Name 
                                + " on "
                                + t.FullName, pi.Name);

                        continue;
                    }

                    if (pi.PropertyType == typeof(DateTime))
                    {
                        logger.Debug("Call setter {0}, convert datetime {1}", pi.Name, val);

                        DateTime dt;
                        if (DateTime.TryParseExact(val, "o", null, System.Globalization.DateTimeStyles.RoundtripKind, out dt))
                        {
                            pi.SetValue(target, dt, null);
                        }
                        else
                        {
                            logger.Warn("Failed to parse {0} as a roundtrip DateTime", val);
                        }
                    }
                    else
                    {
                        logger.Debug("Call setter {0}, convert value {1} to {2}",
                            pi.Name, val, pi.PropertyType);

                        pi.SetValue(target, XmlConfig.ChangeType(val, pi.PropertyType), null);
                    }
                }
            }
            return target;
        }

        protected static bool IsChainRequired(PropertyInfo pi)
        {
            foreach (object o in pi.GetCustomAttributes(true))
                if (o is ChainRequiredAttribute)
                    return true;

            return false;
        }

        /// <summary>
        /// Don't discover properties with XmlIgnoreAttribute, nor the reserved 'Name' property.
        /// </summary>
        /// <param name="pi"></param>
        /// <returns></returns>
        protected static bool ExcludeProperty(PropertyInfo pi)
        {
            if ("Name".Equals(pi.Name))
                return true;

            foreach (object o in pi.GetCustomAttributes(false))
                if (o is XmlIgnoreAttribute)
                    return true;

            return false;
        }

        /// <summary>
        /// Investigate writeable, IConvertible properties of ICommand 'cmd' and
        /// write them into the XML-serializable properties of 'target'
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="target"></param>
        /// <exception cref="ChainXmlSerializationException">If ChainRequired property is absent</exception>
        protected static void SerializeProperties(ICommand cmd, XmlConfig target)
        {
            logger.Trace("Inferring properties of {0}", cmd);

            foreach (PropertyInfo pi in cmd.GetType().GetProperties())
            {
                if (pi.CanWrite && !ExcludeProperty(pi))
                {

                    object val = pi.GetValue(cmd, null);
                    if (val != null && !"".Equals(val))
                    {
                        if (val is DateTime)
                        {
                            DateTime dt = (DateTime)val;

                            target.Properties.Add(
                                new XmlConfigProperty(pi.Name, dt.ToString("o"))
                                );
                        }
                        else if (val is IConvertible || val is Guid)
                        {
                            target.Properties.Add(
                                new XmlConfigProperty(pi.Name, Convert.ToString(val))
                                );
                        }
                        else
                        {
                            logger.Debug("Property {0} of type {1} not IConvertible, DateTime, or Guid",
                                pi.Name, cmd.GetType());
                        }
                    }
                    else if (IsChainRequired(pi))
                    {
                        logger.Info("{0} missing property {1} for serialization", cmd, pi.Name);

                        throw new ChainXmlSerializationException(
                            String.Format("ChainRequired property of {0} is null/blank", cmd),
                            pi.Name
                            );
                    }
                }
            }
        }

        /// <summary>
        /// Returns an Object with the specified Type and whose value is equivalent to the specified object.
        /// Swiped from http://aspalliance.com/852_CodeSnip_ConvertChangeType_Wrapper_that_Handles_Nullable_Types
        /// </summary>
        /// <param name="value">An Object that implements the IConvertible interface.</param>
        /// <param name="conversionType">The Type to which value is to be converted.</param>
        /// <returns>An object whose Type is conversionType (or conversionType's underlying type if conversionType
        /// is Nullable&lt;&gt;) and whose value is equivalent to value. -or- a null reference, if value is a null
        /// reference and conversionType is not a value type.</returns>
        /// <remarks>
        /// This method exists as a workaround to System.Convert.ChangeType(Object, Type) which does not handle
        /// nullables as of version 2.0 (2.0.50727.42) of the .NET Framework. The idea is that this method will
        /// be deleted once Convert.ChangeType is updated in a future version of the .NET Framework to handle
        /// nullable types, so we want this to behave as closely to Convert.ChangeType as possible.
        /// This method was written by Peter Johnson at:
        /// http://aspalliance.com/author.aspx?uId=1026.
        /// </remarks>
        public static object ChangeType(object value, Type conversionType)
        {
            // Note: This if block was taken from Convert.ChangeType as is, and is needed here since we're
            // checking properties on conversionType below.
            if (conversionType == null)
            {
                throw new ArgumentNullException("conversionType");
            } // end if

            // If it's not a nullable type, just pass through the parameters to Convert.ChangeType

            if (conversionType.IsGenericType &&
              conversionType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                // It's a nullable type, so instead of calling Convert.ChangeType directly which would throw a
                // InvalidCastException (per http://weblogs.asp.net/pjohnson/archive/2006/02/07/437631.aspx),
                // determine what the underlying type is
                // If it's null, it won't convert to the underlying type, but that's fine since nulls don't really
                // have a type--so just return null
                // Note: We only do this check if we're converting to a nullable type, since doing it outside
                // would diverge from Convert.ChangeType's behavior, which throws an InvalidCastException if
                // value is null and conversionType is a value type.
                if (value == null)
                {
                    return null;
                } // end if

                // It's a nullable type, and not null, so that means it can be converted to its underlying type,
                // so overwrite the passed-in conversion type with this underlying type
                System.ComponentModel.NullableConverter nullableConverter = new System.ComponentModel.NullableConverter(conversionType);
                conversionType = nullableConverter.UnderlyingType;
            } // end if

            if (conversionType == typeof(System.Guid))
            {
                return new Guid(value.ToString());
            }

            // Now that we've guaranteed conversionType is something Convert.ChangeType can handle (i.e. not a
            // nullable type), pass the call on to Convert.ChangeType
            return Convert.ChangeType(value, conversionType);
        }
    }

    
}
