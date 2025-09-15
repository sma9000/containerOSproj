# Supply Chain Security Notes (Cosign + Policy)

## 1) Sign an image (developer workstation)
cosign generate-key-pair   # creates cosign.key / cosign.pub
cosign sign ghcr.io/<org>/<image>:<tag>

## 2) Verify an image
cosign verify ghcr.io/<org>/<image>:<tag>

## 3) Admission
- Use **Kyverno verifyImages** (see kyverno-verify-images.yaml) to require valid signatures.
- With **Gatekeeper**, commonly restrict registries and enforce digest-pinned images. Signature verification typically requires Kyverno or Sigstore Policy Controller.

## 4) Additional Hardening
- Require **immutable tags** (digests) in workloads.
- Pin base images, run SCA (Trivy/Grype), and SBOM (Syft) in CI.
- Enforce image provenance (SLSA/Attestations) as your program matures.
