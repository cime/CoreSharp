using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CoreSharp.Breeze.Query {

  /**
   * Represents a single expand expand clause that will be part of an EntityQuery. An expand 
   * clause represents the path to other entity types via a navigation path from the current EntityType
   * for a given query. 
   * @author IdeaBlade
   *
   */
  public class ExpandClause {
    private List<string> _propertyPaths;


    public static ExpandClause From(IEnumerable propertyPaths) {
      return (propertyPaths == null) ? null : new ExpandClause(propertyPaths.Cast<string>());
    }

    public ExpandClause(IEnumerable<string> propertyPaths) {
      _propertyPaths = propertyPaths.ToList();
    }


    public IEnumerable<string> PropertyPaths {
      get { return _propertyPaths.AsReadOnly(); }
    }

  }

}