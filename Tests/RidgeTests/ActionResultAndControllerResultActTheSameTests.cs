using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using NUnit.Framework;
using Ridge.Results;
using System;

namespace RidgeTests
{
    public class ActionResultAndControllerResultActTheSameTests
    {
        [Test]
        public void ExceptionIsThrownWhenTypeIsAssignableToIActionResult()
        {
            Exception? actionResultException = null;
            Exception? controllerResultException = null;
            try
            {
                new ActionResult<IActionResult>(new OkResult());
            }
            catch (Exception e)
            {
                actionResultException = e;
            }
            try
            {
                new ControllerResult<IActionResult>(new OkResult());
            }
            catch (Exception e)
            {
                controllerResultException = e;
            }

            controllerResultException.Should().NotBeNull();
            actionResultException.Should().NotBeNull();
            controllerResultException!.Message.Should().Be(actionResultException!.Message);
            controllerResultException.GetType().FullName.Should().Be(actionResultException.GetType().FullName);
            controllerResultException.InnerException.Should().Be(actionResultException.InnerException);
        }

        [Test]
        public void ExceptionIsThrownWhenTypeIsAssignableToIActionResultSecondOverload()
        {
            Exception? actionResultException = null;
            Exception? controllerResultException = null;
            try
            {
                new ActionResult<ActionResult>(new OkResult());
            }
            catch (Exception e)
            {
                actionResultException = e;
            }
            try
            {
                new ControllerResult<ActionResult>(new OkResult());
            }
            catch (Exception e)
            {
                controllerResultException = e;
            }

            controllerResultException.Should().NotBeNull();
            actionResultException.Should().NotBeNull();
            controllerResultException!.Message.Should().Be(actionResultException!.Message);
            controllerResultException.GetType().FullName.Should().Be(actionResultException.GetType().FullName);
            controllerResultException.InnerException.Should().Be(actionResultException.InnerException);
        }

        [Test]
        public void ExceptionIsThrownIfNullIsPassed()
        {
            Exception? actionResultException = null;
            Exception? controllerResultException = null;
            try
            {
                new ActionResult<int>(null);
            }
            catch (Exception e)
            {
                actionResultException = e;
            }
            try
            {
                new ControllerResult<int>(null!);
            }
            catch (Exception e)
            {
                controllerResultException = e;
            }

            controllerResultException.Should().NotBeNull();
            actionResultException.Should().NotBeNull();
            controllerResultException!.Message.Should().Be(actionResultException!.Message);
            controllerResultException.GetType().FullName.Should().Be(actionResultException.GetType().FullName);
            controllerResultException.InnerException.Should().Be(actionResultException.InnerException);
        }


        [Test]
        public void ActionResultContainsCtorWhichTakesTheGenericType()
        {
            var actionResult = new ActionResult<int>(1);
            var controllerResult = new ControllerResult<int>(1);

            var controllerResultInnerValue = ((IConvertToActionResult)controllerResult).Convert();
            var actionResultInnerValue = ((IConvertToActionResult)actionResult).Convert();
            controllerResultInnerValue.Should().BeOfType(actionResultInnerValue.GetType());
            ((int)((ObjectResult)controllerResultInnerValue).Value).Should().Be((int)((ObjectResult)actionResultInnerValue).Value);
            ((ObjectResult)controllerResultInnerValue).DeclaredType.Should().Be(((ObjectResult)actionResultInnerValue).DeclaredType);
        }

        [Test]
        public void ActionResultContainsImplicitCastToTheSpecifiedType()
        {
            ActionResult<int> actionResult = 1;
            ControllerResult<int> controllerResult = 1;

            var controllerResultInnerValue = ((IConvertToActionResult)controllerResult).Convert();
            var actionResultInnerValue = ((IConvertToActionResult)actionResult).Convert();
            controllerResultInnerValue.Should().BeOfType(actionResultInnerValue.GetType());
            ((int)((ObjectResult)controllerResultInnerValue).Value).Should().Be((int)((ObjectResult)actionResultInnerValue).Value);
            ((ObjectResult)controllerResultInnerValue).DeclaredType.Should().Be(((ObjectResult)actionResultInnerValue).DeclaredType);
        }

        [Test]
        public void ActionResultHasCtorWhichTakesActionResult()
        {
            var actionResult = new ActionResult<int>(new OkObjectResult(1));
            var controllerResult = new ControllerResult<int>(new OkObjectResult(1));

            var controllerResultInnerValue = ((IConvertToActionResult)controllerResult).Convert();
            var actionResultInnerValue = ((IConvertToActionResult)actionResult).Convert();
            ((int)((OkObjectResult)controllerResultInnerValue).Value).Should().Be((int)((OkObjectResult)actionResultInnerValue).Value);
        }

        [Test]
        public void ActionResultContainsImplicitCastWhichAllowsCastFromActionResult()
        {
            ActionResult<int> actionResult = new OkObjectResult(1);
            ControllerResult<int> controllerResult = new OkObjectResult(1);

            var controllerResultInnerValue = ((IConvertToActionResult)controllerResult).Convert();
            var actionResultInnerValue = ((IConvertToActionResult)actionResult).Convert();
            ((int)((OkObjectResult)controllerResultInnerValue).Value).Should().Be((int)((OkObjectResult)actionResultInnerValue).Value);
        }
    }
}
