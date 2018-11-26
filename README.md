# UnityPortals  

Working portal system in Unity. Camera can seamlessly move through them.  
The portal object has a MonoBehaviour that renders a cached camera before rendering itself using MonoBehaviour's "OnWillRenderObject", this creates a chain reaction of layers until a fixed amount is reached.  
This amount can be anything from 1 to 16, or if you feel like destroying your memory you could ofcourse set it higher than that.

Can be a bit glitchy when using certain rotations, still working on that
![alt text](https://i.postimg.cc/VLRw6MbZ/portal1.jpg)
