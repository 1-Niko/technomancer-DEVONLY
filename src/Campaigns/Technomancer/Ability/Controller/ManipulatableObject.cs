// Campaigns/Technomancer/Ability/Controller/ManipulatableObject.cs
/* Defines a class to represent all objects Techy can manipulate*/

using UnityEngine;

namespace Slugpack;

public static partial class Technomancer
{
    public static partial class Ability
    {
        public class Owner
        {
            public OwnerType type;
            public object self;
            public ManipulatableObject owner;

            public Owner(object data, ManipulatableObject owner)
            {
                this.owner = owner;
                type = ObjectRegistry.Identify(data);
                self = data;
            }
        }

        public class ManipulatableObject
        {
            public Owner owner; // Refers back to the object it represents
            public int level = 0; // Required karma to effect this object
            public int firewall = 0; // Number of times it will survive being effected before relenting to the player

            public Room room;
            public Vector2 pos;
            public HackNode sprite;

            public int timesTriggered;

            public bool slatedForDeletetion = false;

            public ManipulatableObject(object self, Room room)
            {
                owner = new Owner(self, this);
                this.room = room;
                pos = ObjectRegistry.GetPos(self, owner.type, room);
                level = ObjectRegistry.Level(owner.type);
                firewall = ObjectRegistry.Firewall(owner.type);
            }

            public void AttemptEffect(Input InputType, int PlayerKarma)
            {
                if (PlayerKarma < level) {
                    FailedEffect();
                    return;
                }

                if (this.timesTriggered < this.firewall) {
                    LowerFirewall();
                    FailedEffect();
                    return;
                }

                SuccessfulEffect(InputType);
            }

            public void FailedEffect()
            {

            }

            public void SuccessfulEffect(Input InputType)
            {

            }

            public void LowerFirewall()
            {
                timesTriggered++;
            }
            public void RaiseFirewall()
            {
                timesTriggered--;
            }


            public void Destroy()
            {
                this.slatedForDeletetion = true;
                if (this.sprite != null) {
                    this.sprite?.Destroy();
                    this.sprite.slatedForDeletetion = true;
                    this.sprite = null;
                }
                this.owner = null;
                this.room = null;
            }

            public override bool Equals(object obj)
            {
                if (obj is ManipulatableObject other)
                    return object.Equals(this.owner?.self, other.owner?.self);

                return false;
            }

            public override int GetHashCode()
            {
                return this.owner?.self?.GetHashCode() ?? 0;
            }

            public static bool operator ==(ManipulatableObject lhs, ManipulatableObject rhs)
            {
                if (lhs is null)
                    return rhs is null;
                return lhs.Equals(rhs);
            }

            public static bool operator !=(ManipulatableObject lhs, ManipulatableObject rhs) => !(lhs == rhs);
        }
    }
}