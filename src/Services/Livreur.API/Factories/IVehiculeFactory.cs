using Shared.Contracts.DTOs;

namespace Livreur.API.Factories;

// ISP: split creation from validation
public interface IVehiculeCreator { Models.Vehicule Create(CreateVehiculeDto dto); }
public interface IVehiculeValidator { ValidationResult Validate(CreateVehiculeDto dto); }
public interface IVehiculeFactory : IVehiculeCreator, IVehiculeValidator { }
