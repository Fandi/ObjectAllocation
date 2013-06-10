namespace Belajaran {
	public enum CoroutinePriority {
		HIGH,
		NORMAL,
		LOW,
		WAITFOR_FIXEDUPDATE,
		WAITFOR_ENDOFFRAME
	}
	
	public sealed class ObjectAllocation<T>
		where T : UnityEngine.Object {
		public const CoroutinePriority DEFAULT_COROUTINE_PRIORITY = CoroutinePriority.WAITFOR_ENDOFFRAME;
		
		T original;
		T[] allocation;
		System.Type objectType;
		
		public ObjectAllocation(T original, int size) {
			if (original == default(T)) {
				throw new UnityEngine.UnassignedReferenceException("Argument 'original' is null");
			}
			
			this.original = original;
			allocation = new T[size];
			IsPooled = false;
		}
		
		public T this[int index] {
			get {
				return allocation[index];
			}
			set {
				allocation[index] = value;
			}
		}
		
		public System.Type ObjectType {
			get {
				if (objectType == default(System.Type)) {
					objectType = original.GetType();
				}
				
				return objectType;
			}
		}
		
		public bool IsPooled {
			get;
			private set;
		}
		
		internal UnityEngine.YieldInstruction GetYieldInstruction(CoroutinePriority priority) {
			switch (priority) {
				case CoroutinePriority.HIGH:
					return new UnityEngine.WaitForSeconds(0f);
				case CoroutinePriority.NORMAL:
					return new UnityEngine.WaitForSeconds(500f);
				case CoroutinePriority.LOW:
					return new UnityEngine.WaitForSeconds(1000f);
				case CoroutinePriority.WAITFOR_FIXEDUPDATE:
					return new UnityEngine.WaitForFixedUpdate();
				case CoroutinePriority.WAITFOR_ENDOFFRAME:
					return new UnityEngine.WaitForEndOfFrame();
				default:
					throw new UnityEngine.UnityException("Unhandled AllocationPriority enum value");
			}
		}
		
		public System.Collections.IEnumerator Pool(
			CoroutinePriority priority = DEFAULT_COROUTINE_PRIORITY,
			System.Action<T> oninstantiated = default(System.Action<T>),
			System.Action<ObjectAllocation<T>> onallocated = default(System.Action<ObjectAllocation<T>>)) {
			if (IsPooled) {
				throw new UnityEngine.UnityException("Already pooled");
			}
			
			for (int i = 0; i < allocation.Length; i++) {
				allocation[i] = UnityEngine.Object.Instantiate(original) as T;
			
				if (oninstantiated != default(System.Action<T>)) {
					oninstantiated(allocation[i]);
				}
			
				yield return GetYieldInstruction(priority);
			}
				
			IsPooled = true;
			
			if (onallocated != default(System.Action<ObjectAllocation<T>>)) {
				onallocated(this);
			}
		}
		
		public System.Collections.IEnumerator Iterate(System.Action<T> oniterating,
			CoroutinePriority priority = DEFAULT_COROUTINE_PRIORITY) {
			if (!IsPooled) {
				throw new UnityEngine.UnityException("Not pooled");
			}
			
			if (oniterating == default(System.Action<T>)) {
				throw new UnityEngine.UnassignedReferenceException("Argument 'oniterating' is null");
			}
			
			foreach (T obj in allocation) {
				oniterating(obj);
				yield return GetYieldInstruction(priority);
			}
		}
	}
	
    // System.Object and UnityEngine.Object and Belajaran.Object… Oh my!
	public static class Object {
		public static ObjectAllocation<T> Allocate<T>(T original, int size)
			where T : UnityEngine.Object {
			return new ObjectAllocation<T>(original, size);
		}
	}
}
