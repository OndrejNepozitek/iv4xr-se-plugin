package eu.iv4xr.framework.world;

import java.io.Serializable;
import java.util.*;

import eu.iv4xr.framework.world.WorldModel;
import helperclasses.datastructures.Vec3;

public class WorldEntity {
	
	/**
	 * A unique id identifying this entity.
	 */
	public final String id ;
	
	/**
	 * The type-name of the entity, e.g. "door".
	 */
	public final String type ;
	
	public long timestamp = -1 ;
	
	/**
	 * The center position of this entity,
	 */
	public Vec3 position ;
	/**
	 * Bounding box of this entity.
	 */
	public Vec3 extent ; // bounding box
	public Vec3 velocity ;
	
	/**
	 * If true then this entity can be interacted to.
	 */
	public final boolean interactable ;
	
	/**
	 * If true then this entity is "dynamic", which means that its state may change at the runtime.
	 * Note that an entity does not have to be moving (having velocity) to be dynamic.
	 */
	public final boolean dynamic ;
	public Map<String,Serializable> properties = new HashMap<>();
	
	public Map<String,WorldEntity> elements = new HashMap<>() ;
		
	public WorldEntity(String id, String type, boolean interactable, boolean dynamic) {
		this.id = id ;
		this.type = type ;
		this.interactable = interactable ;
		this.dynamic = dynamic ;
	}
	
	/**
	 * To keep one single copy of the entity's previous state.
	 */
	private WorldEntity previousState = null ;
	
	private boolean equal_(Object a, Object b) {
		if (a==null) return b==null ;
		return a.equals(b) ;
	}
	
	public boolean hasSameState(WorldEntity e) {
		if (! (equal_(position,e.position)
				   && equal_(velocity,e.velocity)
				   && properties.size() == e.properties.size()
				   && equal_(extent,e.extent)))
		    return false ;
		for (var P : properties.entrySet()) {
			var q = e.properties.get(P.getKey()) ;
			if (! P.getValue().equals(q)) return false ;
		}
		// so the entities have the same properties.. let's now check the children 
		if (this.elements.size() != e.elements.size()) return false ;
		for (var elem_ : elements.entrySet()) {
			var elem2 = e.elements.get(elem_.getKey()) ;
			if (elem2 == null) return false ;
			var elem1 = elem_.getValue() ;
			if (!elem1.hasSameState(elem2)) return false ;
		}
		return true ;	
	}
	
	public Serializable getProperty(String propertyName) {
		return properties.get(propertyName) ;
	}
	
	public boolean getBooleanProperty(String propertyName) {
		var V = getProperty(propertyName) ;
		if (V==null) return false ;
		if (!(V instanceof Boolean))
			throw new IllegalArgumentException(id + " has no boolean property " + propertyName) ;
		return (boolean) V ;
	}
	
	public String getStringProperty(String propertyName) {
		var V = getProperty(propertyName) ;
		if (V==null) return null ;
		return V.toString() ;
	}
	
	public int getIntProperty(String propertyName) {
		var V = getProperty(propertyName) ;
		if (V==null || !(V instanceof Integer)) 
			throw new IllegalArgumentException(id + " has no integer property " + propertyName) ;
		return (int) V ;
	}
	
	/**
	 * This will link e as the previous state of this Entity. The previous
	 * state of e is cleared to null (we only
	 * want to track the history of past state up to length 1).
	 * 
	 * This method assume that e represents the same Entity as this Entity
	 * (e.g. they have the same id).
	 */
	public void linkPreviousState(WorldEntity e) {
		this.previousState = e ;
		if (e!=null) e.previousState = null ;
	}
	
	/**
	 * True if this entity has no previous state, or if its state differs
	 * from its previous.
	 * Note that we only track 1x previous state (so there is no longer
	 * chain of previous states).
	 */
	public boolean hasChangedState() {
		if (previousState==null) return true ;
		return !hasSameState(previousState) ;
	}
	
	/**
	 * Set the time-stamp of this Entity and its elements to the given time.
	 */
	public void assignTimeStamp(long ts) {
		timestamp = ts ;
		for (var e : elements.values()) e.assignTimeStamp(ts);
	}
	
	/**
	 * Return true if this entity can block movement, and else false.
	 * This method should be implemented by the subclass. So, override this.
	 */
	public boolean isBlocking() {
		throw new UnsupportedOperationException() ;
	}
	
	/**
	 * If true then this entity is a moving entity. This is defined as having a
	 * non-null velocity. Note that the entity may still have a zero velocity;
	 * but it will still be classified as "moving".
	 */
	public boolean isMovingEntity() { return velocity !=null ; }

}
