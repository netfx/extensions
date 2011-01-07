using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.Windows.Input;

internal class DelegateCommandSpec
{
    public class GivenANonGenericCommand
    {
        [Fact]
        public void WhenNoCanExecuteSpecified_ThenExecutesAlways()
        {
            var executed = 0;
            var command = new DelegateCommand(() => executed++);

            command.Execute(new object());
            command.Execute(new object());

            Assert.Equal(2, executed);
        }

        [Fact]
        public void WhenCanExecuteReturnsFalse_ThenDoesNotExecute()
        {
            var executed = false;
            var command = new DelegateCommand(() => executed = true, () => false);

            command.Execute(new object());

            Assert.False(executed);
        }

        [Fact]
        public void WhenCanExecuteReturnsTrue_ThenExecutes()
        {
            var executed = false;
            var command = new DelegateCommand(() => executed = true, () => true);

            command.Execute(new object());

            Assert.True(executed);
        }

        [Fact]
        public void WhenExecuteParameterIsNull_ThenExecutes()
        {
            var executed = false;
            var command = new DelegateCommand(() => executed = true, () => true);

            command.Execute(null);

            Assert.True(executed);
        }

        [Fact]
        public void WhenCanExecuteParameterIsNull_ThenExecutes()
        {
            var command = new DelegateCommand(() => { }, () => true);

            var can = command.CanExecute(null);

            Assert.True(can);
        }
    }

    public class GivenAGenericCommand
    {
        [Fact]
        public void WhenNoCanExecuteSpecified_ThenExecutesAlways()
        {
            var executed = 0;
            var command = new DelegateCommand<string>(s => executed++);

            command.Execute("foo");
            command.Execute("foo");

            Assert.Equal(2, executed);
        }

        [Fact]
        public void WhenCanExecuteReturnsFalse_ThenDoesNotExecute()
        {
            var executed = false;
            var command = new DelegateCommand<string>(s => executed = true, s => false);

            command.Execute("foo");

            Assert.False(executed);
        }

        [Fact]
        public void WhenCanExecuteReturnsTrue_ThenExecutes()
        {
            var executed = false;
            var command = new DelegateCommand<string>(s => executed = true, s => true);

            command.Execute("foo");

            Assert.True(executed);
        }

        [Fact]
        public void WhenExecuteParameterIsNull_ThenExecutes()
        {
            var executed = false;
            var command = new DelegateCommand<string>(s => executed = true, s => true);

            command.Execute(null);

            Assert.True(executed);
        }

        [Fact]
        public void WhenCanExecuteParameterIsNull_ThenExecutes()
        {
            var command = new DelegateCommand<string>(s => { }, s => true);

            var can = command.CanExecute(null);

            Assert.True(can);
        }

        [Fact]
        public void WhenCanExecuteReceivesParameter_ThenItIsConvertedToType()
        {
            var value = default(string);
            var command = new DelegateCommand<string>(s => {}, s => (value = s) != null);

            var can = command.CanExecute((object)"foo");

            Assert.Equal("foo", value);
        }
    }
}
