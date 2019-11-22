/* Based on Control "CustomPropertyGrid" by Ben Ratzlaff */
using System;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Reflection.Emit;
using System.Threading;
using System.Reflection;
using System.Drawing.Design;

namespace PVEntityGenerator.Controls {

  /// <summary>
  /// A property grid that dynamically generates a Type to conform to desired input
  /// </summary>
  public class PVPropertyGrid : System.Windows.Forms.PropertyGrid {
    private Hashtable typeHash;
    private string typeName="DefType";
    private SettingContainer settings;
    private bool instantUpdate=true;

    public PVPropertyGrid() {
      initTypes();
    }

    [Description("Name of the type that will be internally created")]
    [DefaultValue("DefType")]
    public string TypeName {
      get{return typeName;}
      set{typeName=value;}
    }

    [DefaultValue(true)]
    [Description("If true, the Setting.Update() event will be called when a property changes")]
    public bool InstantUpdate {
      get{return instantUpdate;}
      set{instantUpdate=value;}
    }

    protected override void OnPropertyValueChanged(PropertyValueChangedEventArgs e) {
      base.OnPropertyValueChanged(e);

      ((Setting)settings[e.ChangedItem.Label]).Value=e.ChangedItem.Value;

      if (instantUpdate) {
        ((Setting)settings[e.ChangedItem.Label]).FireUpdate(e);
      }
    }

    [Browsable(false)]
    public SettingContainer Settings {
      set {
        settings=value;

        // Reflection.Emit code below copied and modified from
        // http://longhorn.msdn.microsoft.com/lhsdk/ref/ns/system.reflection.emit/c/propertybuilder/propertybuilder.aspx

        AppDomain myDomain = Thread.GetDomain();
        AssemblyName myAsmName = new AssemblyName();
        myAsmName.Name = "TempAssembly";

        AssemblyBuilder assemblyBuilder = myDomain.DefineDynamicAssembly(myAsmName,AssemblyBuilderAccess.Run);
        ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("TempModule");

        //create our type
        TypeBuilder newType = moduleBuilder.DefineType(typeName, TypeAttributes.Public);

        //create the hashtable used to store property values
        FieldBuilder hashField = newType.DefineField("table",typeof(Hashtable),FieldAttributes.Private);
        createHashMethod(newType.DefineProperty("Hash",PropertyAttributes.None,typeof(Hashtable),new Type[] {}),
          newType,hashField);

        Hashtable h = new Hashtable();
        foreach(string key in settings.Keys) {
          Setting s = settings[key];
          h[key]=s.Value;
          emitProperty(newType,hashField,s,key);
        }

        Type myType = newType.CreateType();
        ConstructorInfo ci = myType.GetConstructor(new Type[]{});
        object o = ci.Invoke(new Object[]{});

        //set the object's hashtable - in the future i would like to do this in the emitted object's constructor
        PropertyInfo pi = myType.GetProperty("Hash");
        pi.SetValue(o,h,null);

        SelectedObject=o;
      }
    }

    private void createHashMethod(PropertyBuilder propBuild,TypeBuilder typeBuild,FieldBuilder hash) {
      // First, we'll define the behavior of the "get" property for Hash as a method.
      MethodBuilder typeHashGet = typeBuild.DefineMethod("GetHash",
        MethodAttributes.Public,
        typeof(Hashtable),
        new Type[] { });
      ILGenerator ilg = typeHashGet.GetILGenerator();
      ilg.Emit(OpCodes.Ldarg_0);
      ilg.Emit(OpCodes.Ldfld, hash);
      ilg.Emit(OpCodes.Ret);

      // Now, we'll define the behavior of the "set" property for Hash.
      MethodBuilder typeHashSet = typeBuild.DefineMethod("SetHash",
        MethodAttributes.Public,
        null,
        new Type[] { typeof(Hashtable) });

      ilg = typeHashSet.GetILGenerator();
      ilg.Emit(OpCodes.Ldarg_0);
      ilg.Emit(OpCodes.Ldarg_1);
      ilg.Emit(OpCodes.Stfld, hash);
      ilg.Emit(OpCodes.Ret);

      // map the two methods created above to their property
      propBuild.SetGetMethod(typeHashGet);
      propBuild.SetSetMethod(typeHashSet);

      //add the [Browsable(false)] property to the Hash property so it doesnt show up on the property list
      ConstructorInfo ci = typeof(BrowsableAttribute).GetConstructor(new Type[]{typeof(bool)});
      CustomAttributeBuilder cab = new CustomAttributeBuilder(ci,new object[]{false});
      propBuild.SetCustomAttribute(cab);
    }

    /// <summary>
    /// Initialize a private hashtable with type-opCode pairs so i dont have to write a long if/else statement when outputting msil
    /// </summary>
    private void initTypes() {
      typeHash=new Hashtable();
      typeHash[typeof(sbyte)]=OpCodes.Ldind_I1;
      typeHash[typeof(byte)]=OpCodes.Ldind_U1;
      typeHash[typeof(char)]=OpCodes.Ldind_U2;
      typeHash[typeof(short)]=OpCodes.Ldind_I2;
      typeHash[typeof(ushort)]=OpCodes.Ldind_U2;
      typeHash[typeof(int)]=OpCodes.Ldind_I4;
      typeHash[typeof(uint)]=OpCodes.Ldind_U4;
      typeHash[typeof(long)]=OpCodes.Ldind_I8;
      typeHash[typeof(ulong)]=OpCodes.Ldind_I8;
      typeHash[typeof(bool)]=OpCodes.Ldind_I1;
      typeHash[typeof(double)]=OpCodes.Ldind_R8;
      typeHash[typeof(float)]=OpCodes.Ldind_R4;
    }

    /// <summary>
    /// emits a generic get/set property in which the result returned resides in a hashtable whos key is the name of the property
    /// </summary>
    /// <param name="pb">PropertyBuilder used to emit</param>
    /// <param name="tb">TypeBuilder of the class</param>
    /// <param name="hash">FieldBuilder of the hashtable used to store the object</param>
    /// <param name="po">PropertyObject of this property</param>
    private void emitProperty(TypeBuilder tb,FieldBuilder hash,Setting s,string name) {
      //to figure out what opcodes to emit, i would compile a small class having the functionality i wanted, and viewed it with ildasm.
      //peverify is also kinda nice to use to see what errors there are.

      //define the property first
      PropertyBuilder pb = tb.DefineProperty(name,PropertyAttributes.None,s.Value.GetType(),new Type[] {});
      Type objType = s.Value.GetType();

      //now we define the get method for the property
      MethodBuilder getMethod = tb.DefineMethod("get_"+name,MethodAttributes.Public,objType,new Type[]{});
      ILGenerator ilg = getMethod.GetILGenerator();
      ilg.DeclareLocal(objType);
      ilg.Emit(OpCodes.Ldarg_0);
      ilg.Emit(OpCodes.Ldfld,hash);
      ilg.Emit(OpCodes.Ldstr,name);

      ilg.EmitCall(OpCodes.Callvirt,typeof(Hashtable).GetMethod("get_Item"),null);
      if(objType.IsValueType) {
        ilg.Emit(OpCodes.Unbox,objType);
        if(typeHash[objType]!=null)
          ilg.Emit((OpCode)typeHash[objType]);
        else
          ilg.Emit(OpCodes.Ldobj,objType);
      }
      else
        ilg.Emit(OpCodes.Castclass,objType);

      ilg.Emit(OpCodes.Stloc_0);
      ilg.Emit(OpCodes.Br_S,(byte)0);
      ilg.Emit(OpCodes.Ldloc_0);
      ilg.Emit(OpCodes.Ret);

      //now we generate the set method for the property
      MethodBuilder setMethod = tb.DefineMethod("set_"+name,MethodAttributes.Public,null,new Type[]{objType});
      ilg = setMethod.GetILGenerator();
      ilg.Emit(OpCodes.Ldarg_0);
      ilg.Emit(OpCodes.Ldfld,hash);
      ilg.Emit(OpCodes.Ldstr,name);
      ilg.Emit(OpCodes.Ldarg_1);
      if(objType.IsValueType)
        ilg.Emit(OpCodes.Box,objType);
      ilg.EmitCall(OpCodes.Callvirt,typeof(Hashtable).GetMethod("set_Item"),null);
      ilg.Emit(OpCodes.Ret);

      //put the get/set methods in with the property
      pb.SetGetMethod(getMethod);
      pb.SetSetMethod(setMethod);

      //if we specified a description, we will now add the DescriptionAttribute to our property
      if (s.Description!=null) {
        ConstructorInfo ci = typeof(DescriptionAttribute).GetConstructor(new Type[]{typeof(string)});
        CustomAttributeBuilder cab = new CustomAttributeBuilder(ci,new object[]{s.Description});
        pb.SetCustomAttribute(cab);
      }

      //add a CategoryAttribute if specified
      if (s.Category!=null) {
        ConstructorInfo ci = typeof(CategoryAttribute).GetConstructor(new Type[]{typeof(string)});
        CustomAttributeBuilder cab = new CustomAttributeBuilder(ci,new object[]{s.Category});
        pb.SetCustomAttribute(cab);
      }

      //UI Type Editor
      if (s.Editor!=null) {
        ConstructorInfo ci = typeof(EditorAttribute).GetConstructor(new Type[]{typeof(System.Type), typeof(System.Type)});
        CustomAttributeBuilder cab = new CustomAttributeBuilder(ci,new object[]{s.Editor.GetType(), typeof(UITypeEditor)});
        pb.SetCustomAttribute(cab);
      }
    }


    /// <summary>
    /// A wrapper around a Hashtable for Setting objects. Setting objects are intended to use with the CustomPropertyGrid
    /// </summary>
    public class SettingContainer {
      private Hashtable settings;

      public SettingContainer() {
        settings = new Hashtable();
      }

      /// <summary>
      /// Get the key collection for this Settings object. Every key is a string
      /// </summary>
      public ICollection Keys {
        get{return settings.Keys;}
      }

      /// <summary>
      /// Get/Set the Setting object tied to the input string
      /// </summary>
      public Setting this[string key] {
        get{return (Setting)settings[key];}
        set{settings[key]=value;value.Name=key;}
      }

      /*
            /// <summary>
            /// Gets the Setting object tied to the string. If there is no Setting object, one will be created with the defaultValue
            /// </summary>
            /// <param name="key">The name of the setting object</param>
            /// <param name="defaultvalue">if there is no Setting object tied to the string, a Setting will be created with this as its Value</param>
            /// <returns>The Setting object tied to the string</returns>
            public Setting GetSetting(string key, object defaultvalue) {
              if(settings[key]==null) {
                settings[key]=new Setting(defaultvalue,null,null);
                ((Setting)settings[key]).Name=key;
              }

              return (Setting)settings[key];
            }
      */
    }


    public delegate void SettingEventHandler(Setting pSetting, PropertyValueChangedEventArgs pArgs);

    /// <summary>
    /// Stores information to be displayed in the CustomPropertyGrid
    /// </summary>
    public class Setting {
      private object val;
      private string desc,category,name,key;
      private UITypeEditor editor;

      public event SettingEventHandler ValueChanged;

      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="key">Internal name/key of setting</param>
      /// <param name="val">The current value of the setting</param>
      /// <param name="desc">The setting's description</param>
      /// <param name="category">The setting's category</param>
      /// <param name="update">An eventhandler that will be called if CustomPropertyGrid.InstantUpdate is true</param>
      /// <param name="editor">Editor for Property</param>
      public Setting(string key,string desc,string category,object val,SettingEventHandler update,
          UITypeEditor editor) {
        this.key=key;
        this.desc=desc;
        this.category=category;
        this.val=val;
        if (update!=null) {
          ValueChanged+=update;
        }
        this.editor = editor;
      }

      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="val">The current value of the setting</param>
      /// <param name="desc">The setting's description</param>
      /// <param name="category">The setting's category</param>
      /// <param name="update">An eventhandler that will be called if CustomPropertyGrid.InstantUpdate is true</param>
      public Setting(string key,string desc,string category,object val,SettingEventHandler update)
        : this (key, desc, category, val, update, null) {}

      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="val">The current value of the setting</param>
      /// <param name="desc">The setting's description</param>
      /// <param name="category">The setting's category</param>
      public Setting(string key,string desc,string category,object val):this(key,desc,category,val,null){}

      #region get/set properties for the private data
      public object Value {
        get{return val;}
        set{val=value;}
      }

      public string Description {
        get{return desc;}
        set{desc=value;}
      }

      public string Category {
        get{return category;}
        set{category=value;}
      }

      public string Name {
        get{return name;}
        set{name=value;}
      }

      public string Key {
        get{return key;}
        set{key=value;}
      }

      public UITypeEditor Editor {
        get {return editor;}
        set {editor=value;}
      }
      #endregion

      /// <summary>
      /// Allows an external object to force calling the event
      /// </summary>
      /// <param name="e"></param>
      public void FireUpdate(PropertyValueChangedEventArgs e) {
        //I didnt do this in the Value's set method because sometimes I want to set the Value without firing the event
        //I could do the same thing with a second property, but this works fine.
        if (ValueChanged!=null) {
          ValueChanged(this, e);
        }
      }
    }

  }

}
