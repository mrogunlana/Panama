﻿using Panama.Extensions;
using Panama.Interfaces;
using Panama.Tests.MySQL.Contexts;
using Panama.Tests.MySQL.Models;
using System.Threading.Tasks;

namespace Panama.Tests.MySQL.Commands.EF
{
    public class DeleteUsers : ICommand
    {
        private readonly AppDbContext _context;

        public DeleteUsers(AppDbContext context)
        {
            _context = context;
        }
        public async Task Execute(IContext context)
        {
            var users = context.DataGet<User>();

            _context.RemoveRange(users);

            await _context.SaveChangesAsync();
        }
    }
}
