﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.Model
{
    public class Account : INotifyPropertyChanged
    {
        protected int _id;
        protected string _name;
        protected string _phoneNumber;
        private string _address;
        protected string _username;
        protected string _password; // non encrypted

        public int Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string PhoneNumber { get => _phoneNumber; set => _phoneNumber = value; }
        public string Address { get => _address; set => _address = value; }
        public string Username { get => _username; set => _username = value; }
        public string Password { get => _password; set => _password = value; }
        public string Entropy { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
