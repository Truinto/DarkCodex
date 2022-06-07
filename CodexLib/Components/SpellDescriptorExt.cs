using Kingmaker.Blueprints.Classes.Spells;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    [Obsolete("discarded")]
    public class SpellDescriptorExt : SpellDescriptorComponent
    {
        public BitArray DescriptorExt;

        public SpellDescriptorExt()
        {
            DescriptorExt = new(128, false);
        }

        public SpellDescriptorExt(SpellDescriptor descriptor)
        {
            int low = (int)descriptor;
            int high = (int)((long)descriptor >> 32);
            DescriptorExt = new(new int[] { low, high, 0, 0 });
            this.Descriptor = descriptor;
        }

        public SpellDescriptorExt(params int[] indices)
        {
            DescriptorExt = new(128, false);
            foreach (int index in indices)
                DescriptorExt[index] = true;
            Update();
        }

        public void AddDescriptor(SpellDescriptor descriptor)
        {
            for (int i = 0; i < 64; i++)
                if (((long)descriptor & 1L >> i) != 0)
                    DescriptorExt[i] = true;
            Update();
        }

        public void AddDescritor(int index)
        {
            DescriptorExt.Set(index, true);
            if (index < 64)
                Update();
        }

        public void RemoveDescriptor(SpellDescriptor descriptor)
        {
            for (int i = 0; i < 64; i++)
                if (((long)descriptor & 1L >> i) != 0)
                    DescriptorExt[i] = false;
            Update();
        }

        public void RemoveDescriptor(int index)
        {
            DescriptorExt.Set(index, false);
            if (index < 64)
                Update();
        }

        public bool HasAnyFlags(SpellDescriptor descriptor)
        {
            for (int i = 0; i < 64; i++)
                if (((long)descriptor & 1L >> i) != 0)
                    if (DescriptorExt[i])
                        return true;
            return false;
        }

        public bool HasAnyFlags(params int[] indices)
        {
            foreach (var index in indices)
                if (DescriptorExt[index])
                    return true;
            return false;
        }

        public bool HasAllFlags(SpellDescriptor descriptor)
        {
            for (int i = 0; i < 64; i++)
                if (((long)descriptor & 1L >> i) != 0)
                    if (!DescriptorExt[i])
                        return false;
            return true;
        }

        public bool HasAllFlags(params int[] indices)
        {
            foreach (var index in indices)
                if (!DescriptorExt[index])
                    return false;
            return true;
        }

        private unsafe void Update()
        {
            DescriptorExt.CopyTo(buffer, 0);
            this.Descriptor = (SpellDescriptor)buffer[0];
        }

        public static int Register(string id)
        {
            return counter++;
        }

        private static int counter = 64;
        private static int[] buffer = new int[4];
    }
}