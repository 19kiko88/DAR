import { TestBed } from '@angular/core/testing';

import { RdaService } from './rda.service';

describe('RdaService', () => {
  let service: RdaService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(RdaService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
